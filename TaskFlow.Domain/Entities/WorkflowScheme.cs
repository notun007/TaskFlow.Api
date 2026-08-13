using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Workflow;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

public sealed class WorkflowScheme : Entity
{
    public required string Name { get; set; }
    public Guid? WorkItemTypeId { get; set; }
    public WorkItemType? WorkItemType { get; set; }
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public bool IsDefault { get; set; }
    public ICollection<WorkflowTransition> Transitions { get; set; } = [];
}

public sealed class WorkflowTransition : Entity
{
    public Guid WorkflowSchemeId { get; set; }
    public WorkflowScheme WorkflowScheme { get; set; } = null!;
    public WorkflowStatus FromStatus { get; set; }
    public WorkflowStatus ToStatus { get; set; }
    public int SortOrder { get; set; }
}

public static class BuiltInWorkflow
{
    public static readonly Guid SchemeId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly DateTimeOffset SeededAt = new(2026, 8, 9, 0, 0, 0, TimeSpan.Zero);
    public static IReadOnlyList<(Guid Id, WorkflowStatus From, WorkflowStatus To, int SortOrder)> Transitions =>
        DefaultTaskWorkflow.AllTransitions.Select((transition, index) =>
            (Guid.Parse($"20000000-0000-0000-0001-{index + 1:x12}"), transition.From, transition.To, (index + 1) * 10)).ToArray();
}
