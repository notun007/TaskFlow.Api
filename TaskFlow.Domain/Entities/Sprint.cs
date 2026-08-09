using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public enum SprintStatus { Planned, Active, Completed }

public sealed class Sprint : Entity
{
    public required string Name { get; set; }
    public string? Goal { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public SprintStatus Status { get; set; } = SprintStatus.Planned;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
