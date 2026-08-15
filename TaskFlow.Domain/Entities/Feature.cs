using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public enum FeatureStatus { Active, Completed, Cancelled }

public sealed class Feature : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid EpicId { get; set; }
    public Epic Epic { get; set; } = null!;
    public FeatureStatus Status { get; set; } = FeatureStatus.Active;
    public DateOnly? TargetDate { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
