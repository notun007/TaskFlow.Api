using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Workflow;
using TaskFlow.Domain.Entities;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;
using Xunit;

namespace TaskFlow.Domain.Tests.Workflow;

public sealed class DefaultTaskWorkflowTests
{
    [Theory]
    [InlineData(WorkflowStatus.Submitted, WorkflowStatus.Triaged)]
    [InlineData(WorkflowStatus.Triaged, WorkflowStatus.Approved)]
    [InlineData(WorkflowStatus.Assigned, WorkflowStatus.InProgress)]
    [InlineData(WorkflowStatus.InProgress, WorkflowStatus.ReadyForTesting)]
    [InlineData(WorkflowStatus.ReadyForTesting, WorkflowStatus.Uat)]
    [InlineData(WorkflowStatus.Resolved, WorkflowStatus.Closed)]
    [InlineData(WorkflowStatus.Closed, WorkflowStatus.Reopened)]
    public void CanTransition_AllowsConfiguredPath(WorkflowStatus current, WorkflowStatus target) =>
        Assert.True(DefaultTaskWorkflow.CanTransition(current, target));

    [Theory]
    [InlineData(WorkflowStatus.Submitted, WorkflowStatus.Closed)]
    [InlineData(WorkflowStatus.Triaged, WorkflowStatus.Resolved)]
    [InlineData(WorkflowStatus.Approved, WorkflowStatus.Closed)]
    [InlineData(WorkflowStatus.Closed, WorkflowStatus.InProgress)]
    public void CanTransition_RejectsStatusJump(WorkflowStatus current, WorkflowStatus target) =>
        Assert.False(DefaultTaskWorkflow.CanTransition(current, target));

    [Fact]
    public void AllowedTransitions_DoesNotAllowSameStatus()
    {
        foreach (var status in Enum.GetValues<WorkflowStatus>())
            Assert.DoesNotContain(status, DefaultTaskWorkflow.AllowedTransitions(status));
    }

    [Fact]
    public void Seeded_workflow_preserves_every_default_transition()
    {
        Assert.Equal(DefaultTaskWorkflow.AllTransitions.Count, BuiltInWorkflow.Transitions.Count);
        Assert.Equal(BuiltInWorkflow.Transitions.Count, BuiltInWorkflow.Transitions.Select(x => x.Id).Distinct().Count());
        Assert.All(BuiltInWorkflow.Transitions, item => Assert.Contains((item.From, item.To), DefaultTaskWorkflow.AllTransitions));
    }

    [Fact]
    public void Team_member_default_scope_requires_the_current_user_to_be_the_task_assignee() =>
        Assert.Equal(TaskAccessScope.AssigneeIsCurrentUser, UniversalTransitionRolePolicy.DefaultScope(ProjectRole.TeamMember));
}
