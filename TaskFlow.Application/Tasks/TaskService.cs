using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Tasks;

public sealed record TaskListItem(Guid Id, string TaskNumber, string Title, TaskType Type, WorkflowStatus Status, Priority Priority, DateTimeOffset? DueDate, string ProjectName);
public sealed record CreateTaskRequest(string Title, string? Description, TaskType Type, Priority Priority, Severity? Severity, Guid ProjectId, Guid? SoftwareApplicationId, DateTimeOffset? DueDate);

public interface ITaskService
{
    Task<IReadOnlyList<TaskListItem>> ListAsync(string? search, WorkflowStatus? status, CancellationToken cancellationToken);
    Task<TaskItem?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<TaskItem> CreateAsync(CreateTaskRequest request, string actor, CancellationToken cancellationToken);
    Task<bool> ChangeStatusAsync(Guid id, WorkflowStatus status, string actor, CancellationToken cancellationToken);
}

public sealed class TaskService(IApplicationDbContext db) : ITaskService
{
    public async Task<IReadOnlyList<TaskListItem>> ListAsync(string? search, WorkflowStatus? status, CancellationToken cancellationToken)
    {
        var query = db.Tasks.AsNoTracking().Include(x => x.Project).Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.TaskNumber.Contains(search));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return await query.OrderBy(x => x.DueDate).Select(x => new TaskListItem(x.Id, x.TaskNumber, x.Title, x.Type, x.Status, x.Priority, x.DueDate, x.Project!.Name)).ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> GetAsync(Guid id, CancellationToken cancellationToken) => db.Tasks.Include(x => x.Assignments).Include(x => x.Comments).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<TaskItem> CreateAsync(CreateTaskRequest request, string actor, CancellationToken cancellationToken)
    {
        var task = new TaskItem { TaskNumber = $"TF-{DateTime.UtcNow:yyyyMMddHHmmss}", Title = request.Title, Description = request.Description, Type = request.Type, Priority = request.Priority, Severity = request.Severity, ProjectId = request.ProjectId, SoftwareApplicationId = request.SoftwareApplicationId, DueDate = request.DueDate, Status = WorkflowStatus.Submitted };
        db.Tasks.Add(task);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = task.Id.ToString(), Action = "Created", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<bool> ChangeStatusAsync(Guid id, WorkflowStatus status, string actor, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (task is null) return false;
        task.Status = status; task.UpdatedAt = DateTimeOffset.UtcNow;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = $"StatusChanged:{status}", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
