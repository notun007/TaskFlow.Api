using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Configuration;

public sealed class BuiltInWorkItemTypesTests
{
    [Fact]
    public void BuiltInTypes_HaveUniqueKeysNamesAndSortOrders()
    {
        Assert.Equal(BuiltInWorkItemTypes.All.Count, BuiltInWorkItemTypes.All.Select(item => item.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(BuiltInWorkItemTypes.All.Count, BuiltInWorkItemTypes.All.Select(item => item.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(BuiltInWorkItemTypes.All.Count, BuiltInWorkItemTypes.All.Select(item => item.SortOrder).Distinct().Count());
    }

    [Theory]
    [InlineData("Bug")]
    [InlineData("Requirement")]
    [InlineData("ChangeRequest")]
    [InlineData("Incident")]
    public void BuiltInTypes_ContainExistingTaskKeys(string key) =>
        Assert.Contains(BuiltInWorkItemTypes.All, item => item.Key == key);
}
