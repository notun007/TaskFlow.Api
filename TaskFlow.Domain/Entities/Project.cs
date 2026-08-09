using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public sealed class Project : Entity
{
    public required string Name { get; set; }
    public string? ProjectKey { get; set; }
    public string? Objectives { get; set; }
    public string Status { get; set; } = "Active";
    public DateOnly? StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? ProjectManager { get; set; }
    public string? Sponsor { get; set; }
    public Guid? SoftwareApplicationId { get; set; }
    public SoftwareApplication? SoftwareApplication { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<Sprint> Sprints { get; set; } = [];
    public ICollection<ProjectRelease> Releases { get; set; } = [];
}
