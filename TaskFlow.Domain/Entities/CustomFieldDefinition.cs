using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public enum CustomFieldType
{
    Text,
    LongText,
    Number,
    Date,
    Boolean,
    Select,
    MultiSelect,
    User,
    Team
}

public sealed class CustomFieldDefinition : Entity
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public CustomFieldType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public ICollection<CustomFieldOption> Options { get; set; } = [];
    public ICollection<CustomFieldContext> Contexts { get; set; } = [];

    public bool RequiresOptions => Type is CustomFieldType.Select or CustomFieldType.MultiSelect;
}

public sealed class CustomFieldOption : Entity
{
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
    public required string Value { get; set; }
    public required string Label { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CustomFieldContext : Entity
{
    public Guid CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
    public Guid? WorkItemTypeId { get; set; }
    public WorkItemType? WorkItemType { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string SectionName { get; set; } = "Additional information";
    public bool ShowOnCreate { get; set; } = true;
    public bool ShowOnEdit { get; set; } = true;
    public bool ShowOnDetails { get; set; } = true;
}
