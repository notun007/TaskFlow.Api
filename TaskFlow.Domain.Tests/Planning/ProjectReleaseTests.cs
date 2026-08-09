using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Planning;

public sealed class ProjectReleaseTests
{
    [Fact]
    public void New_release_is_unreleased()
    {
        var release = new ProjectRelease { Name = "2026.1" };
        Assert.Equal(ReleaseStatus.Unreleased, release.Status);
        Assert.Null(release.ReleasedAt);
    }

    [Fact]
    public void Release_can_collect_fix_version_tasks()
    {
        var release = new ProjectRelease { Name = "2026.1" };
        release.Tasks.Add(new TaskItem { TaskNumber = "TF-1", Title = "Task", Type = "Bug" });
        Assert.Single(release.Tasks);
    }
}
