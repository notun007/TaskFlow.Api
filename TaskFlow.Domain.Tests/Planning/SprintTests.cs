using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Planning;

public sealed class SprintTests
{
    [Fact]
    public void New_sprint_is_planned()
    {
        var sprint = new Sprint { Name = "Sprint 1" };
        Assert.Equal(SprintStatus.Planned, sprint.Status);
        Assert.Null(sprint.StartedAt);
        Assert.Null(sprint.CompletedAt);
    }

    [Fact]
    public void Sprint_can_collect_project_tasks()
    {
        var sprint = new Sprint { Name = "Sprint 1" };
        sprint.Tasks.Add(new TaskItem { TaskNumber = "TF-1", Title = "Task", Type = "Bug" });
        Assert.Single(sprint.Tasks);
    }
}
