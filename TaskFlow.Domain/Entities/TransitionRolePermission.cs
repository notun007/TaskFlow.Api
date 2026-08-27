using TaskFlow.Domain.Common;
using TaskFlow.Domain.Workflow;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

public sealed class TransitionRolePermission : Entity
{
    public WorkflowStatus FromStatus { get; set; }
    public WorkflowStatus ToStatus { get; set; }
    public ProjectRole Role { get; set; }
    public TaskAccessScope TaskScope { get; set; } = TaskAccessScope.AllProjectTasks;
}

public enum TaskAccessScope
{
    AllProjectTasks,
    ReportedByCurrentUser,
    OwnedByCurrentUser,
    AssignedToCurrentUser,
    PrimaryAssignedToCurrentUser,
    AssigneeIsCurrentUser,
    TesterIsCurrentUser
}

public static class UniversalTransitionRolePolicy
{
    public static readonly DateTimeOffset SeededAt = new(2026, 8, 13, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<(Guid Id, WorkflowStatus From, WorkflowStatus To, ProjectRole Role, TaskAccessScope TaskScope)> Permissions
    {
        get
        {
            var permissions = new HashSet<(WorkflowStatus From, WorkflowStatus To, ProjectRole Role)>();
            AddFrom(permissions, WorkflowStatus.Draft, ProjectRole.Requester);
            Add(permissions, WorkflowStatus.Submitted, WorkflowStatus.Triaged, ProjectRole.ProductOwner, ProjectRole.TeamLead);
            AddFrom(permissions, WorkflowStatus.Submitted, ProjectRole.ProductOwner);
            AddFrom(permissions, WorkflowStatus.Triaged, ProjectRole.ProductOwner);
            AddFrom(permissions, WorkflowStatus.Approved, ProjectRole.TeamLead);
            AddFrom(permissions, WorkflowStatus.Assigned, ProjectRole.TeamMember);
            AddFrom(permissions, WorkflowStatus.InProgress, ProjectRole.TeamMember);
            AddFrom(permissions, WorkflowStatus.PendingInformation, ProjectRole.TeamMember);
            AddFrom(permissions, WorkflowStatus.PendingVendor, ProjectRole.TeamMember);
            AddFrom(permissions, WorkflowStatus.ReadyForTesting, ProjectRole.ReviewerTester);
            AddFrom(permissions, WorkflowStatus.Uat, ProjectRole.ReviewerTester);
            AddFrom(permissions, WorkflowStatus.Resolved, ProjectRole.ProductOwner, ProjectRole.Requester);
            AddFrom(permissions, WorkflowStatus.Closed, ProjectRole.ProductOwner, ProjectRole.Requester);
            AddFrom(permissions, WorkflowStatus.Rejected, ProjectRole.ProductOwner, ProjectRole.Requester);
            AddFrom(permissions, WorkflowStatus.Cancelled, ProjectRole.ProductOwner, ProjectRole.Requester);
            AddFrom(permissions, WorkflowStatus.Reopened, ProjectRole.TeamMember);

            foreach (var transition in DefaultTaskWorkflow.AllTransitions)
                permissions.Add((transition.From, transition.To, ProjectRole.ProjectAdmin));

            return permissions.OrderBy(x => x.From).ThenBy(x => x.To).ThenBy(x => x.Role)
                .Select((item, index) => (Guid.Parse($"30000000-0000-0000-0001-{index + 1:x12}"), item.From, item.To, item.Role, DefaultScope(item.Role)))
                .ToArray();
        }
    }

    private static void AddFrom(HashSet<(WorkflowStatus From, WorkflowStatus To, ProjectRole Role)> permissions, WorkflowStatus from, params ProjectRole[] roles)
    {
        foreach (var to in DefaultTaskWorkflow.AllowedTransitions(from)) Add(permissions, from, to, roles);
    }

    private static void Add(HashSet<(WorkflowStatus From, WorkflowStatus To, ProjectRole Role)> permissions, WorkflowStatus from, WorkflowStatus to, params ProjectRole[] roles)
    {
        foreach (var role in roles) permissions.Add((from, to, role));
    }

    public static TaskAccessScope DefaultScope(ProjectRole role) => role switch
    {
        ProjectRole.Requester => TaskAccessScope.ReportedByCurrentUser,
        ProjectRole.TeamMember => TaskAccessScope.AssigneeIsCurrentUser,
        ProjectRole.ReviewerTester => TaskAccessScope.TesterIsCurrentUser,
        _ => TaskAccessScope.AllProjectTasks
    };
}
