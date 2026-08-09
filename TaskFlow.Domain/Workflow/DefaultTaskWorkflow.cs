using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Workflow;

public static class DefaultTaskWorkflow
{
    private static readonly IReadOnlyDictionary<WorkflowStatus, WorkflowStatus[]> Transitions =
        new Dictionary<WorkflowStatus, WorkflowStatus[]>
        {
            [WorkflowStatus.Draft] = [WorkflowStatus.Submitted, WorkflowStatus.Cancelled],
            [WorkflowStatus.Submitted] = [WorkflowStatus.Triaged, WorkflowStatus.Rejected, WorkflowStatus.Cancelled],
            [WorkflowStatus.Triaged] = [WorkflowStatus.Approved, WorkflowStatus.PendingInformation, WorkflowStatus.Rejected, WorkflowStatus.Cancelled],
            [WorkflowStatus.Approved] = [WorkflowStatus.Assigned, WorkflowStatus.Cancelled],
            [WorkflowStatus.Assigned] = [WorkflowStatus.InProgress, WorkflowStatus.PendingInformation, WorkflowStatus.PendingVendor, WorkflowStatus.Cancelled],
            [WorkflowStatus.InProgress] = [WorkflowStatus.PendingInformation, WorkflowStatus.PendingVendor, WorkflowStatus.ReadyForTesting, WorkflowStatus.Resolved, WorkflowStatus.Cancelled],
            [WorkflowStatus.PendingInformation] = [WorkflowStatus.Triaged, WorkflowStatus.Assigned, WorkflowStatus.InProgress, WorkflowStatus.Cancelled],
            [WorkflowStatus.PendingVendor] = [WorkflowStatus.Assigned, WorkflowStatus.InProgress, WorkflowStatus.Cancelled],
            [WorkflowStatus.ReadyForTesting] = [WorkflowStatus.Uat, WorkflowStatus.InProgress, WorkflowStatus.Reopened],
            [WorkflowStatus.Uat] = [WorkflowStatus.Resolved, WorkflowStatus.InProgress, WorkflowStatus.Reopened],
            [WorkflowStatus.Resolved] = [WorkflowStatus.Closed, WorkflowStatus.Reopened],
            [WorkflowStatus.Closed] = [WorkflowStatus.Reopened],
            [WorkflowStatus.Rejected] = [WorkflowStatus.Reopened],
            [WorkflowStatus.Cancelled] = [WorkflowStatus.Reopened],
            [WorkflowStatus.Reopened] = [WorkflowStatus.Triaged, WorkflowStatus.Assigned, WorkflowStatus.InProgress, WorkflowStatus.Cancelled]
        };

    public static IReadOnlyList<WorkflowStatus> AllowedTransitions(WorkflowStatus current) =>
        Transitions.TryGetValue(current, out var statuses) ? statuses : [];

    public static bool CanTransition(WorkflowStatus current, WorkflowStatus target) =>
        AllowedTransitions(current).Contains(target);

    public static IReadOnlyList<(WorkflowStatus From, WorkflowStatus To)> AllTransitions =>
        Transitions.SelectMany(pair => pair.Value.Select(target => (pair.Key, target))).ToArray();
}
