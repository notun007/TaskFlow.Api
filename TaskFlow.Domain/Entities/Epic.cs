using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public enum EpicStatus { Active, Completed, Cancelled }

public sealed class Epic : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public EpicStatus Status { get; set; } = EpicStatus.Active;
    public DateOnly? TargetDate { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
