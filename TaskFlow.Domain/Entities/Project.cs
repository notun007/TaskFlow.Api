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
    public WorkflowScheme? WorkflowScheme { get; set; }
    public ProjectBoard? Board { get; set; }
    public ICollection<ProjectRoleAssignment> RoleAssignments { get; set; } = [];
}

public enum ProjectRole
{
    Requester,
    ProductOwner,
    TeamLead,
    TeamMember,
    ReviewerTester,
    ProjectAdmin
}

public sealed class ProjectRoleAssignment : Entity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid UserId { get; set; }
    public ProjectRole Role { get; set; }
}
