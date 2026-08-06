using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Tasks;

public sealed record TaskListItem(Guid Id, string TaskNumber, string Title, TaskType Type, WorkflowStatus Status, Priority Priority, DateTimeOffset? DueDate, string ProjectName);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
public sealed record CreateTaskRequest(string Title, string? Description, TaskType Type, Priority Priority, Severity? Severity, Guid ProjectId, Guid? SoftwareApplicationId, DateTimeOffset? DueDate);
public sealed record UpdateTaskRequest(string Title, string? Description, TaskType Type, Priority Priority, Severity? Severity, Guid ProjectId, Guid? SoftwareApplicationId, DateTimeOffset? DueDate);
public sealed record AddTaskAssignmentRequest(ResponsibilityType Responsibility, string PartyReference, string? DisplayName);
public sealed record TaskAssignmentItem(Guid Id, ResponsibilityType Responsibility, string PartyReference, string? DisplayName);
public sealed record TaskCommentItem(Guid Id, string AuthorReference, string Body, DateTimeOffset CreatedAt);
public sealed record TaskDetails(
    Guid Id,
    string TaskNumber,
    string Title,
    string? Description,
    TaskType Type,
    WorkflowStatus Status,
    Priority Priority,
    Severity? Severity,
    Guid ProjectId,
    string ProjectName,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<TaskAssignmentItem> Assignments,
    IReadOnlyList<TaskCommentItem> Comments);

public interface ITaskService
{
    Task<PagedResult<TaskListItem>> ListAsync(string? search, WorkflowStatus? status, Priority? priority, Guid? projectId, string? sortBy, string? sortDirection, int page, int pageSize, CancellationToken cancellationToken);
    Task<TaskDetails?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<TaskItem> CreateAsync(CreateTaskRequest request, string actor, CancellationToken cancellationToken);
    Task<TaskDetails?> UpdateAsync(Guid id, UpdateTaskRequest request, string actor, CancellationToken cancellationToken);
    Task<bool> ChangeStatusAsync(Guid id, WorkflowStatus status, string actor, CancellationToken cancellationToken);
    Task<TaskCommentItem?> AddCommentAsync(Guid id, string body, string actor, CancellationToken cancellationToken);
    Task<TaskAssignmentItem?> AddAssignmentAsync(Guid id, AddTaskAssignmentRequest request, string actor, CancellationToken cancellationToken);
    Task<bool> RemoveAssignmentAsync(Guid id, Guid assignmentId, string actor, CancellationToken cancellationToken);
}

public sealed class TaskService(IApplicationDbContext db) : ITaskService
{
    public async Task<PagedResult<TaskListItem>> ListAsync(string? search, WorkflowStatus? status, Priority? priority, Guid? projectId, string? sortBy, string? sortDirection, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Tasks.AsNoTracking().Include(x => x.Project).Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.TaskNumber.Contains(search));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (priority.HasValue) query = query.Where(x => x.Priority == priority.Value);
        if (projectId.HasValue) query = query.Where(x => x.ProjectId == projectId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy?.ToLowerInvariant(), descending) switch
        {
            ("title", true) => query.OrderByDescending(x => x.Title),
            ("title", false) => query.OrderBy(x => x.Title),
            ("priority", true) => query.OrderByDescending(x => x.Priority),
            ("priority", false) => query.OrderBy(x => x.Priority),
            ("status", true) => query.OrderByDescending(x => x.Status),
            ("status", false) => query.OrderBy(x => x.Status),
            (_, true) => query.OrderByDescending(x => x.DueDate),
            _ => query.OrderBy(x => x.DueDate)
        };
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 5, 100);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new TaskListItem(x.Id, x.TaskNumber, x.Title, x.Type, x.Status, x.Priority, x.DueDate, x.Project!.Name))
            .ToListAsync(cancellationToken);
        return new PagedResult<TaskListItem>(items, totalCount, page, pageSize);
    }

    public async Task<TaskDetails?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.Assignments)
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (task is null) return null;

        return new TaskDetails(
            task.Id,
            task.TaskNumber,
            task.Title,
            task.Description,
            task.Type,
            task.Status,
            task.Priority,
            task.Severity,
            task.ProjectId,
            task.Project?.Name ?? "Unassigned project",
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt,
            task.Assignments.Where(x => !x.IsDeleted).Select(x => new TaskAssignmentItem(x.Id, x.Responsibility, x.PartyReference, x.DisplayName)).ToArray(),
            task.Comments.OrderBy(x => x.CreatedAt).Select(x => new TaskCommentItem(x.Id, x.AuthorReference, x.Body, x.CreatedAt)).ToArray());
    }

    public async Task<TaskItem> CreateAsync(CreateTaskRequest request, string actor, CancellationToken cancellationToken)
    {
        var task = new TaskItem { TaskNumber = $"TF-{DateTime.UtcNow:yyyyMMddHHmmss}", Title = request.Title, Description = request.Description, Type = request.Type, Priority = request.Priority, Severity = request.Severity, ProjectId = request.ProjectId, SoftwareApplicationId = request.SoftwareApplicationId, DueDate = request.DueDate, Status = WorkflowStatus.Submitted };
        db.Tasks.Add(task);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = task.Id.ToString(), Action = "Created", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskDetails?> UpdateAsync(Guid id, UpdateTaskRequest request, string actor, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (task is null || string.IsNullOrWhiteSpace(request.Title)) return null;
        task.Title = request.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        task.Type = request.Type;
        task.Priority = request.Priority;
        task.Severity = request.Severity;
        task.ProjectId = request.ProjectId;
        task.SoftwareApplicationId = request.SoftwareApplicationId;
        task.DueDate = request.DueDate;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = "Updated", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
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

    public async Task<TaskCommentItem?> AddCommentAsync(Guid id, string body, string actor, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body) || !await db.Tasks.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)) return null;
        var comment = new TaskComment { TaskItemId = id, AuthorReference = actor, Body = body.Trim() };
        db.TaskComments.Add(comment);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = "CommentAdded", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return new TaskCommentItem(comment.Id, comment.AuthorReference, comment.Body, comment.CreatedAt);
    }

    public async Task<TaskAssignmentItem?> AddAssignmentAsync(Guid id, AddTaskAssignmentRequest request, string actor, CancellationToken cancellationToken)
    {
        var partyReference = request.PartyReference.Trim();
        if (string.IsNullOrWhiteSpace(partyReference) || !await db.Tasks.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)) return null;
        var existing = await db.TaskAssignments.FirstOrDefaultAsync(x => x.TaskItemId == id && x.Responsibility == request.Responsibility && x.PartyReference == partyReference && !x.IsDeleted, cancellationToken);
        if (existing is not null) return new TaskAssignmentItem(existing.Id, existing.Responsibility, existing.PartyReference, existing.DisplayName);
        var assignment = new TaskAssignment { TaskItemId = id, Responsibility = request.Responsibility, PartyReference = partyReference, DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim() };
        db.TaskAssignments.Add(assignment);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = $"AssignmentAdded:{request.Responsibility}", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return new TaskAssignmentItem(assignment.Id, assignment.Responsibility, assignment.PartyReference, assignment.DisplayName);
    }

    public async Task<bool> RemoveAssignmentAsync(Guid id, Guid assignmentId, string actor, CancellationToken cancellationToken)
    {
        var assignment = await db.TaskAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId && x.TaskItemId == id && !x.IsDeleted, cancellationToken);
        if (assignment is null) return false;
        assignment.IsDeleted = true;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = $"AssignmentRemoved:{assignment.Responsibility}", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
