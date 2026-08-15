using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Workflow;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Tasks;

public sealed record TaskListItem(Guid Id, string TaskNumber, string Title, string Type, WorkflowStatus Status, Priority Priority, DateTimeOffset? DueDate, string ProjectName, Guid? EpicId, string? EpicName);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
public sealed record SaveTaskCustomFieldValueRequest(Guid CustomFieldDefinitionId, string? Value);
public sealed record CreateTaskRequest(string Title, string? Description, string Type, Priority Priority, Severity? Severity, Guid ProjectId, Guid? SoftwareApplicationId, DateTimeOffset? DueDate, Guid? EpicId, IReadOnlyList<SaveTaskCustomFieldValueRequest>? CustomFields);
public sealed record UpdateTaskRequest(string Title, string? Description, string Type, Priority Priority, Severity? Severity, Guid ProjectId, Guid? SoftwareApplicationId, DateTimeOffset? DueDate, Guid? EpicId, IReadOnlyList<SaveTaskCustomFieldValueRequest>? CustomFields);
public sealed record AddTaskAssignmentRequest(ResponsibilityType Responsibility, string PartyReference, string? DisplayName);
public sealed record TaskAssignmentItem(Guid Id, ResponsibilityType Responsibility, string PartyReference, string? DisplayName);
public sealed record TaskCommentItem(Guid Id, string AuthorReference, string Body, DateTimeOffset CreatedAt);
public sealed record TaskStatusHistoryItem(Guid Id, WorkflowStatus FromStatus, WorkflowStatus ToStatus, string ActorReference, string? Comment, DateTimeOffset CreatedAt);
public sealed record TaskCustomFieldValueItem(Guid CustomFieldDefinitionId, string Key, string Name, string Type, string? Value, IReadOnlyList<string> DisplayValues);
public sealed record AddTaskLinkRequest(TaskLinkType Type, string TargetTaskReference);
public sealed record TaskLinkItem(Guid Id, TaskLinkType Type, bool IsOutgoing, Guid OtherTaskId, string OtherTaskNumber, string OtherTaskTitle, WorkflowStatus OtherTaskStatus);
public sealed record TaskAttachmentItem(Guid Id, string FileName, string ContentType, long Size, string UploadedBy, DateTimeOffset CreatedAt);
public enum TaskStatusChangeOutcome { Changed, NotFound, InvalidTransition, Forbidden }
public sealed record TaskStatusChangeResult(TaskStatusChangeOutcome Outcome, TaskDetails? Task, IReadOnlyList<WorkflowStatus> AllowedTransitions, IReadOnlyList<ProjectRole> RequiredRoles);
public sealed record TaskDetails(
    Guid Id,
    string TaskNumber,
    string Title,
    string? Description,
    string Type,
    WorkflowStatus Status,
    Priority Priority,
    Severity? Severity,
    Guid ProjectId,
    string ProjectName,
    Guid? EpicId,
    string? EpicName,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<WorkflowStatus> AllowedTransitions,
    IReadOnlyList<TaskStatusHistoryItem> StatusHistory,
    IReadOnlyList<TaskCustomFieldValueItem> CustomFields,
    IReadOnlyList<TaskLinkItem> Links,
    IReadOnlyList<TaskAttachmentItem> Attachments,
    IReadOnlyList<TaskAssignmentItem> Assignments,
    IReadOnlyList<TaskCommentItem> Comments);

public interface ITaskService
{
    Task<PagedResult<TaskListItem>> ListAsync(string? search, WorkflowStatus? status, Priority? priority, Guid? projectId, Guid? epicId, string? epicAssignment, string? sortBy, string? sortDirection, int page, int pageSize, CancellationToken cancellationToken);
    Task<TaskDetails?> GetAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null);
    Task<TaskItem?> CreateAsync(CreateTaskRequest request, string actor, CancellationToken cancellationToken);
    Task<TaskDetails?> UpdateAsync(Guid id, UpdateTaskRequest request, string actor, CancellationToken cancellationToken);
    Task<TaskStatusChangeResult> ChangeStatusAsync(Guid id, WorkflowStatus status, string? comment, string actor, CancellationToken cancellationToken, Guid? userId = null);
    Task<TaskCommentItem?> AddCommentAsync(Guid id, string body, string actor, CancellationToken cancellationToken);
    Task<TaskAssignmentItem?> AddAssignmentAsync(Guid id, AddTaskAssignmentRequest request, string actor, CancellationToken cancellationToken);
    Task<bool> RemoveAssignmentAsync(Guid id, Guid assignmentId, string actor, CancellationToken cancellationToken);
    Task<TaskLinkItem?> AddLinkAsync(Guid id, AddTaskLinkRequest request, string actor, CancellationToken cancellationToken);
    Task<bool> RemoveLinkAsync(Guid id, Guid linkId, string actor, CancellationToken cancellationToken);
}

public sealed class TaskService(IApplicationDbContext db) : ITaskService
{
    public async Task<PagedResult<TaskListItem>> ListAsync(string? search, WorkflowStatus? status, Priority? priority, Guid? projectId, Guid? epicId, string? epicAssignment, string? sortBy, string? sortDirection, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Tasks.AsNoTracking().Include(x => x.Project).Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.TaskNumber.Contains(search));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (priority.HasValue) query = query.Where(x => x.Priority == priority.Value);
        if (projectId.HasValue) query = query.Where(x => x.ProjectId == projectId.Value);
        if (epicId.HasValue) query = query.Where(x => x.EpicId == epicId.Value);
        else if (string.Equals(epicAssignment, "assigned", StringComparison.OrdinalIgnoreCase)) query = query.Where(x => x.EpicId != null);
        else if (string.Equals(epicAssignment, "unassigned", StringComparison.OrdinalIgnoreCase)) query = query.Where(x => x.EpicId == null);

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
            .Select(x => new TaskListItem(x.Id, x.TaskNumber, x.Title, x.Type, x.Status, x.Priority, x.DueDate, x.Project!.Name, x.EpicId, x.Epic != null ? x.Epic.Name : null))
            .ToListAsync(cancellationToken);
        return new PagedResult<TaskListItem>(items, totalCount, page, pageSize);
    }

    public async Task<TaskDetails?> GetAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null)
    {
        var task = await db.Tasks.AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.Epic)
            .Include(x => x.Assignments)
            .Include(x => x.Comments)
            .Include(x => x.StatusHistory)
            .Include(x => x.CustomFieldValues).ThenInclude(x => x.CustomFieldDefinition).ThenInclude(x => x.Options)
            .Include(x => x.CustomFieldValues).ThenInclude(x => x.CustomFieldDefinition).ThenInclude(x => x.Contexts).ThenInclude(x => x.WorkItemType)
            .Include(x => x.OutgoingLinks).ThenInclude(x => x.TargetTask)
            .Include(x => x.IncomingLinks).ThenInclude(x => x.SourceTask)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (task is null) return null;
        var attachments = await db.TaskAttachments.AsNoTracking().Where(x => x.TaskItemId == id && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).Select(x => new TaskAttachmentItem(x.Id, x.FileName, x.ContentType, x.Size, x.UploadedBy, x.CreatedAt)).ToArrayAsync(cancellationToken);

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
            task.EpicId,
            task.Epic != null ? task.Epic.Name : null,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt,
            await AllowedTransitionsAsync(task.ProjectId, task.Status, userId, cancellationToken),
            task.StatusHistory.OrderByDescending(x => x.CreatedAt).Select(x => new TaskStatusHistoryItem(x.Id, x.FromStatus, x.ToStatus, x.ActorReference, x.Comment, x.CreatedAt)).ToArray(),
            task.CustomFieldValues.Where(x => !x.IsDeleted && x.CustomFieldDefinition.Contexts.Any(c => !c.IsDeleted && c.ShowOnDetails && (c.WorkItemTypeId == null || c.WorkItemType!.Key == task.Type))).OrderBy(x => x.CustomFieldDefinition.SortOrder).Select(x => new TaskCustomFieldValueItem(x.CustomFieldDefinitionId, x.CustomFieldDefinition.Key, x.CustomFieldDefinition.Name, x.CustomFieldDefinition.Type.ToString(), x.Value, DisplayValues(x))).ToArray(),
            task.OutgoingLinks.Where(x => !x.IsDeleted).Select(x => new TaskLinkItem(x.Id, x.Type, true, x.TargetTaskId, x.TargetTask.TaskNumber, x.TargetTask.Title, x.TargetTask.Status))
                .Concat(task.IncomingLinks.Where(x => !x.IsDeleted).Select(x => new TaskLinkItem(x.Id, x.Type, false, x.SourceTaskId, x.SourceTask.TaskNumber, x.SourceTask.Title, x.SourceTask.Status))).ToArray(),
            attachments,
            task.Assignments.Where(x => !x.IsDeleted).Select(x => new TaskAssignmentItem(x.Id, x.Responsibility, x.PartyReference, x.DisplayName)).ToArray(),
            task.Comments.OrderBy(x => x.CreatedAt).Select(x => new TaskCommentItem(x.Id, x.AuthorReference, x.Body, x.CreatedAt)).ToArray());
    }

    public async Task<TaskItem?> CreateAsync(CreateTaskRequest request, string actor, CancellationToken cancellationToken)
    {
        var type = request.Type.Trim();
        if (string.IsNullOrWhiteSpace(request.Title) || !await db.WorkItemTypes.AnyAsync(item => !item.IsDeleted && item.IsActive && item.Key == type, cancellationToken)) return null;
        var fields = await ApplicableFields(type, cancellationToken);
        if (!ValidateCustomFields(fields, request.CustomFields, true)) return null;
        if (!await ValidEpic(request.EpicId, request.ProjectId, cancellationToken)) return null;
        var initialStatus = await InitialStatusAsync(request.ProjectId, cancellationToken);
        var task = new TaskItem { TaskNumber = $"TF-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..27].ToUpperInvariant(), Title = request.Title.Trim(), Description = request.Description, Type = type, Priority = request.Priority, Severity = request.Severity, ProjectId = request.ProjectId, SoftwareApplicationId = request.SoftwareApplicationId, DueDate = request.DueDate, EpicId = request.EpicId, Status = initialStatus };
        db.Tasks.Add(task);
        SyncCustomFields(task, fields, request.CustomFields, true);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = task.Id.ToString(), Action = "Created", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskDetails?> UpdateAsync(Guid id, UpdateTaskRequest request, string actor, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.Include(x => x.CustomFieldValues).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (task is null || string.IsNullOrWhiteSpace(request.Title)) return null;
        var type = request.Type.Trim();
        if (!await db.WorkItemTypes.AnyAsync(item => !item.IsDeleted && item.IsActive && item.Key == type, cancellationToken)) return null;
        var fields = await ApplicableFields(type, cancellationToken);
        if (!ValidateCustomFields(fields, request.CustomFields, false)) return null;
        if (!await ValidEpic(request.EpicId, request.ProjectId, cancellationToken)) return null;
        task.Title = request.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        task.Type = type;
        task.Priority = request.Priority;
        task.Severity = request.Severity;
        task.ProjectId = request.ProjectId;
        task.SoftwareApplicationId = request.SoftwareApplicationId;
        task.DueDate = request.DueDate;
        task.EpicId = request.EpicId;
        SyncCustomFields(task, fields, request.CustomFields, false);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = "Updated", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public async Task<TaskStatusChangeResult> ChangeStatusAsync(Guid id, WorkflowStatus status, string? comment, string actor, CancellationToken cancellationToken, Guid? userId = null)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (task is null) return new(TaskStatusChangeOutcome.NotFound, null, [], []);
        var workflowAllowed = await WorkflowTransitionsAsync(task.ProjectId, task.Status, cancellationToken);
        var allowed = await AllowedTransitionsAsync(task.ProjectId, task.Status, userId, cancellationToken);
        if (!workflowAllowed.Contains(status))
            return new(TaskStatusChangeOutcome.InvalidTransition, null, allowed, []);
        if (!allowed.Contains(status))
            return new(TaskStatusChangeOutcome.Forbidden, null, allowed, await RequiredRolesAsync(task.Status, status, cancellationToken));
        var fromStatus = task.Status;
        var transitionComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (transitionComment?.Length > 2000) transitionComment = transitionComment[..2000];
        task.Status = status; task.UpdatedAt = DateTimeOffset.UtcNow;
        db.TaskStatusHistory.Add(new TaskStatusHistory { TaskItemId = id, FromStatus = fromStatus, ToStatus = status, ActorReference = actor, Comment = transitionComment });
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = $"StatusChanged:{status}", ActorReference = actor, ChangesJson = JsonSerializer.Serialize(new { fromStatus, toStatus = status, comment = transitionComment }) });
        await db.SaveChangesAsync(cancellationToken);
        return new(TaskStatusChangeOutcome.Changed, await GetAsync(id, cancellationToken, userId), await AllowedTransitionsAsync(task.ProjectId, status, userId, cancellationToken), []);
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

    public async Task<TaskLinkItem?> AddLinkAsync(Guid id, AddTaskLinkRequest request, string actor, CancellationToken cancellationToken)
    {
        var reference = request.TargetTaskReference.Trim();
        var source = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        var target = await db.Tasks.FirstOrDefaultAsync(x => !x.IsDeleted && (x.TaskNumber == reference || x.Id.ToString() == reference), cancellationToken);
        if (source is null || target is null || source.Id == target.Id) return null;
        var existing = await db.TaskLinks.FirstOrDefaultAsync(x => x.SourceTaskId == source.Id && x.TargetTaskId == target.Id && x.Type == request.Type && !x.IsDeleted, cancellationToken);
        if (existing is not null) return new(existing.Id, existing.Type, true, target.Id, target.TaskNumber, target.Title, target.Status);
        if (request.Type == TaskLinkType.ParentOf && await db.TaskLinks.AnyAsync(x => x.TargetTaskId == target.Id && x.Type == TaskLinkType.ParentOf && !x.IsDeleted, cancellationToken)) return null;
        var link = new TaskLink { SourceTaskId = source.Id, TargetTaskId = target.Id, Type = request.Type };
        db.TaskLinks.Add(link);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = $"LinkAdded:{request.Type}", ActorReference = actor, ChangesJson = JsonSerializer.Serialize(new { target.Id, target.TaskNumber }) });
        await db.SaveChangesAsync(cancellationToken);
        return new(link.Id, link.Type, true, target.Id, target.TaskNumber, target.Title, target.Status);
    }

    public async Task<bool> RemoveLinkAsync(Guid id, Guid linkId, string actor, CancellationToken cancellationToken)
    {
        var link = await db.TaskLinks.FirstOrDefaultAsync(x => x.Id == linkId && (x.SourceTaskId == id || x.TargetTaskId == id) && !x.IsDeleted, cancellationToken);
        if (link is null) return false;
        link.IsDeleted = true;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = $"LinkRemoved:{link.Type}", ActorReference = actor });
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<List<CustomFieldDefinition>> ApplicableFields(string typeKey, CancellationToken cancellationToken)
    {
        var typeId = await db.WorkItemTypes.Where(x => x.Key == typeKey && x.IsActive).Select(x => x.Id).SingleAsync(cancellationToken);
        return await db.CustomFieldDefinitions.Include(x => x.Options).Include(x => x.Contexts)
            .Where(x => x.IsActive && x.Contexts.Any(c => !c.IsDeleted && (c.WorkItemTypeId == null || c.WorkItemTypeId == typeId)))
            .ToListAsync(cancellationToken);
    }

    private async Task<bool> ValidEpic(Guid? epicId, Guid projectId, CancellationToken cancellationToken) =>
        !epicId.HasValue || await db.Epics.AnyAsync(x => x.Id == epicId.Value && x.ProjectId == projectId && x.Status == EpicStatus.Active, cancellationToken);

    private async Task<IReadOnlyList<WorkflowStatus>> WorkflowTransitionsAsync(Guid projectId, WorkflowStatus current, CancellationToken cancellationToken)
    {
        var schemeId = await db.WorkflowSchemes.Where(x => x.ProjectId == projectId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? await db.WorkflowSchemes.Where(x => x.IsDefault).Select(x => (Guid?)x.Id).SingleAsync(cancellationToken);
        return await db.WorkflowTransitions.AsNoTracking().Where(x => x.WorkflowSchemeId == schemeId && x.FromStatus == current)
            .OrderBy(x => x.SortOrder).Select(x => x.ToStatus).ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<WorkflowStatus>> AllowedTransitionsAsync(Guid projectId, WorkflowStatus current, Guid? userId, CancellationToken cancellationToken)
    {
        var workflowAllowed = await WorkflowTransitionsAsync(projectId, current, cancellationToken);
        if (!userId.HasValue || workflowAllowed.Count == 0) return [];
        var roles = await db.ProjectRoleAssignments.AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.UserId == userId.Value)
            .Select(x => x.Role).ToListAsync(cancellationToken);
        if (roles.Count == 0) return [];
        var permitted = await db.TransitionRolePermissions.AsNoTracking()
            .Where(x => x.FromStatus == current && workflowAllowed.Contains(x.ToStatus) && roles.Contains(x.Role))
            .Select(x => x.ToStatus).Distinct().ToListAsync(cancellationToken);
        return workflowAllowed.Where(permitted.Contains).ToArray();
    }

    private async Task<IReadOnlyList<ProjectRole>> RequiredRolesAsync(WorkflowStatus from, WorkflowStatus to, CancellationToken cancellationToken) =>
        await db.TransitionRolePermissions.AsNoTracking().Where(x => x.FromStatus == from && x.ToStatus == to)
            .OrderBy(x => x.Role).Select(x => x.Role).Distinct().ToListAsync(cancellationToken);

    private async Task<WorkflowStatus> InitialStatusAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var projectSchemeId = await db.WorkflowSchemes
            .Where(x => x.ProjectId == projectId)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (projectSchemeId.HasValue)
        {
            var projectEntryStatus = await db.WorkflowTransitions.AsNoTracking()
                .Where(x => x.WorkflowSchemeId == projectSchemeId.Value)
                .OrderBy(x => x.SortOrder)
                .Select(x => (WorkflowStatus?)x.FromStatus)
                .FirstOrDefaultAsync(cancellationToken);
            if (projectEntryStatus.HasValue) return projectEntryStatus.Value;
        }

        var defaultSchemeId = await db.WorkflowSchemes
            .Where(x => x.IsDefault)
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);
        return await db.WorkflowTransitions.AsNoTracking()
            .Where(x => x.WorkflowSchemeId == defaultSchemeId)
            .OrderBy(x => x.SortOrder)
            .Select(x => (WorkflowStatus?)x.FromStatus)
            .FirstOrDefaultAsync(cancellationToken)
            ?? WorkflowStatus.Draft;
    }

    private static bool ValidateCustomFields(IReadOnlyList<CustomFieldDefinition> fields, IReadOnlyList<SaveTaskCustomFieldValueRequest>? requests, bool createScreen)
    {
        requests ??= [];
        if (requests.GroupBy(x => x.CustomFieldDefinitionId).Any(x => x.Count() > 1)) return false;
        if (requests.Any(x => fields.All(f => f.Id != x.CustomFieldDefinitionId))) return false;
        foreach (var field in fields)
        {
            var request = requests.FirstOrDefault(x => x.CustomFieldDefinitionId == field.Id);
            var value = request?.Value?.Trim();
            var context = field.Contexts.First(c => !c.IsDeleted);
            var appearsOnScreen = createScreen ? context.ShowOnCreate : context.ShowOnEdit;
            if (appearsOnScreen && context.IsRequired && string.IsNullOrWhiteSpace(value)) return false;
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (!CustomFieldValueValidator.IsValid(field, value)) return false;
        }
        return true;
    }

    private static void SyncCustomFields(TaskItem task, IReadOnlyList<CustomFieldDefinition> fields, IReadOnlyList<SaveTaskCustomFieldValueRequest>? requests, bool createScreen)
    {
        requests ??= [];
        foreach (var field in fields)
        {
            var context = field.Contexts.First(x => !x.IsDeleted);
            if (!(createScreen ? context.ShowOnCreate : context.ShowOnEdit)) continue;
            var value = requests.FirstOrDefault(x => x.CustomFieldDefinitionId == field.Id)?.Value?.Trim();
            if (createScreen && string.IsNullOrWhiteSpace(value)) value = context.DefaultValue;
            var existing = task.CustomFieldValues.FirstOrDefault(x => x.CustomFieldDefinitionId == field.Id);
            if (string.IsNullOrWhiteSpace(value)) { if (existing is not null) existing.IsDeleted = true; continue; }
            if (existing is null) task.CustomFieldValues.Add(new TaskCustomFieldValue { CustomFieldDefinitionId = field.Id, Value = value });
            else { existing.Value = value; existing.IsDeleted = false; }
        }
    }

    private static IReadOnlyList<string> DisplayValues(TaskCustomFieldValue value)
    {
        if (string.IsNullOrWhiteSpace(value.Value)) return [];
        var values = value.CustomFieldDefinition.Type == CustomFieldType.MultiSelect
            ? JsonSerializer.Deserialize<string[]>(value.Value) ?? [] : [value.Value];
        var labels = value.CustomFieldDefinition.Options.Where(x => !x.IsDeleted).ToDictionary(x => x.Value, x => x.Label, StringComparer.OrdinalIgnoreCase);
        return values.Select(x => labels.GetValueOrDefault(x, x)).ToArray();
    }
}
