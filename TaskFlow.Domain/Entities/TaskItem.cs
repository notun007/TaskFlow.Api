using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

public sealed class TaskItem : Entity
{
    public required string TaskNumber { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Type { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    public Priority Priority { get; set; } = Priority.Medium;
    public Severity? Severity { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid? EpicId { get; set; }
    public Epic? Epic { get; set; }
    public Guid? FeatureId { get; set; }
    public Feature? Feature { get; set; }
    public Guid? SprintId { get; set; }
    public Sprint? Sprint { get; set; }
    public Guid? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> Subtasks { get; set; } = [];
    public Guid? FixVersionId { get; set; }
    public ProjectRelease? FixVersion { get; set; }
    public Guid? SoftwareApplicationId { get; set; }
    public SoftwareApplication? SoftwareApplication { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? OwnerDisplayName { get; set; }
    public Guid? ReporterUserId { get; set; }
    public string? ReporterDisplayName { get; set; }
    public int? EstimatedEffortMinutes { get; set; }
    public string? Environment { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? SlaDueDate { get; set; }
    public string? Resolution { get; set; }
    public string? Impact { get; set; }
    public string? ReproductionSteps { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
    public string? RootCause { get; set; }
    public string? Workaround { get; set; }
    public string? Source { get; set; }
    public string? SourceReference { get; set; }
    public ICollection<TaskAssignment> Assignments { get; set; } = [];
    public ICollection<TaskComment> Comments { get; set; } = [];
    public ICollection<TaskStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<TaskCustomFieldValue> CustomFieldValues { get; set; } = [];
    public ICollection<TaskLink> OutgoingLinks { get; set; } = [];
    public ICollection<TaskLink> IncomingLinks { get; set; } = [];
    public ICollection<TaskAttachment> Attachments { get; set; } = [];
}

public sealed class TaskAttachment : Entity
{
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long Size { get; set; }
    public required byte[] Content { get; set; }
    public required string UploadedBy { get; set; }
}

public enum TaskLinkType { Blocks, RelatesTo, Duplicates, ParentOf }

public sealed class TaskLink : Entity
{
    public Guid SourceTaskId { get; set; }
    public TaskItem SourceTask { get; set; } = null!;
    public Guid TargetTaskId { get; set; }
    public TaskItem TargetTask { get; set; } = null!;
    public TaskLinkType Type { get; set; }
}

public sealed class TaskCustomFieldValue : Entity
{
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
    public string? Value { get; set; }
}

public sealed class TaskAssignment : Entity
{
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public required ResponsibilityType Responsibility { get; set; }
    public required string PartyReference { get; set; }
    public string? DisplayName { get; set; }
    public bool IsPrimary { get; set; }
}

public sealed class TaskComment : Entity
{
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public required string AuthorReference { get; set; }
    public required string Body { get; set; }
}

public sealed class TaskStatusHistory : Entity
{
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public WorkflowStatus FromStatus { get; set; }
    public WorkflowStatus ToStatus { get; set; }
    public required string ActorReference { get; set; }
    public string? Comment { get; set; }
}

public sealed class AuditEntry : Entity
{
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public required string Action { get; set; }
    public required string ActorReference { get; set; }
    public string? ChangesJson { get; set; }
    public string? IpAddress { get; set; }
}
