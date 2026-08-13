using TaskFlow.Domain.Common;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

public sealed class ProjectBoard : Entity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public ICollection<ProjectBoardColumn> Columns { get; set; } = [];
}

public sealed class ProjectBoardColumn : Entity
{
    public Guid ProjectBoardId { get; set; }
    public ProjectBoard ProjectBoard { get; set; } = null!;
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public int? WipLimit { get; set; }
    public WorkflowStatus Status { get; set; }
    public bool IsDefaultDestination { get; set; } = true;
}
