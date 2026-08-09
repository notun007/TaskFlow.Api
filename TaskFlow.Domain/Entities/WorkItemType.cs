using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public sealed class WorkItemType : Entity
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }
}

public sealed record BuiltInWorkItemType(Guid Id, string Key, string Name, string Description, int SortOrder);

public static class BuiltInWorkItemTypes
{
    public static readonly DateTimeOffset SeededAt = new(2026, 8, 9, 0, 0, 0, TimeSpan.Zero);
    public static readonly IReadOnlyList<BuiltInWorkItemType> All =
    [
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"), "Bug", "Bug", "A defect or unexpected system behavior.", 10),
        new(Guid.Parse("10000000-0000-0000-0000-000000000002"), "Requirement", "Requirement", "A business or functional requirement.", 20),
        new(Guid.Parse("10000000-0000-0000-0000-000000000003"), "ChangeRequest", "Change request", "A controlled request to change an existing service.", 30),
        new(Guid.Parse("10000000-0000-0000-0000-000000000004"), "Enhancement", "Enhancement", "An improvement to an existing capability.", 40),
        new(Guid.Parse("10000000-0000-0000-0000-000000000005"), "Incident", "Incident", "An operational incident requiring restoration.", 50),
        new(Guid.Parse("10000000-0000-0000-0000-000000000006"), "MeetingAction", "Meeting action", "An action captured from a meeting or committee.", 60),
        new(Guid.Parse("10000000-0000-0000-0000-000000000007"), "Testing", "Testing", "Testing or quality-assurance work.", 70),
        new(Guid.Parse("10000000-0000-0000-0000-000000000008"), "Deployment", "Deployment", "A deployment or release activity.", 80),
        new(Guid.Parse("10000000-0000-0000-0000-000000000009"), "Maintenance", "Maintenance", "Planned system or service maintenance.", 90)
    ];
}
