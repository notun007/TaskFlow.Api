using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Tasks;

public sealed class TaskLinkTests
{
    [Fact]
    public void Link_connects_two_distinct_tasks()
    {
        var source = Guid.NewGuid(); var target = Guid.NewGuid();
        var link = new TaskLink { SourceTaskId = source, TargetTaskId = target, Type = TaskLinkType.Blocks };
        Assert.NotEqual(link.SourceTaskId, link.TargetTaskId);
        Assert.Equal(TaskLinkType.Blocks, link.Type);
    }

    [Fact]
    public void Supported_link_types_include_hierarchy_and_dependencies()
    {
        Assert.Contains(TaskLinkType.ParentOf, Enum.GetValues<TaskLinkType>());
        Assert.Contains(TaskLinkType.Blocks, Enum.GetValues<TaskLinkType>());
        Assert.Contains(TaskLinkType.RelatesTo, Enum.GetValues<TaskLinkType>());
    }
}
