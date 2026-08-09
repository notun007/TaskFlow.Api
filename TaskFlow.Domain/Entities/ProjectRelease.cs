using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public enum ReleaseStatus { Unreleased, Released, Archived }

public sealed class ProjectRelease : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public ReleaseStatus Status { get; set; } = ReleaseStatus.Unreleased;
    public DateOnly? StartDate { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
