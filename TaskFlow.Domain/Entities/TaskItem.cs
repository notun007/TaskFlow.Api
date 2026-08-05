using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

public sealed class TaskItem : Entity
{
    public required string TaskNumber { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskType Type { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    public Priority Priority { get; set; } = Priority.Medium;
    public Severity? Severity { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid? SoftwareApplicationId { get; set; }
    public SoftwareApplication? SoftwareApplication { get; set; }
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
}

public sealed class TaskAssignment : Entity
{
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public required ResponsibilityType Responsibility { get; set; }
    public required string PartyReference { get; set; }
    public string? DisplayName { get; set; }
}

public sealed class TaskComment : Entity
{
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public required string AuthorReference { get; set; }
    public required string Body { get; set; }
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
