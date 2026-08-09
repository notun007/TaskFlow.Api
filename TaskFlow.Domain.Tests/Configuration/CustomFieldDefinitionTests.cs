using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Configuration;

public sealed class CustomFieldDefinitionTests
{
    [Theory]
    [InlineData(CustomFieldType.Select)]
    [InlineData(CustomFieldType.MultiSelect)]
    public void Option_backed_types_require_options(CustomFieldType type)
    {
        var field = new CustomFieldDefinition { Key = "Risk", Name = "Risk", Type = type };
        Assert.True(field.RequiresOptions);
    }

    [Theory]
    [InlineData(CustomFieldType.Text)]
    [InlineData(CustomFieldType.Number)]
    [InlineData(CustomFieldType.Date)]
    public void Scalar_types_do_not_require_options(CustomFieldType type)
    {
        var field = new CustomFieldDefinition { Key = "Risk", Name = "Risk", Type = type };
        Assert.False(field.RequiresOptions);
    }

    [Fact]
    public void Select_value_must_exist_in_active_options()
    {
        var field = new CustomFieldDefinition { Key = "Risk", Name = "Risk", Type = CustomFieldType.Select,
            Options = [new CustomFieldOption { Value = "High", Label = "High" }] };
        Assert.True(CustomFieldValueValidator.IsValid(field, "High"));
        Assert.False(CustomFieldValueValidator.IsValid(field, "Unknown"));
    }

    [Theory]
    [InlineData(CustomFieldType.Number, "12.5", true)]
    [InlineData(CustomFieldType.Number, "twelve", false)]
    [InlineData(CustomFieldType.Boolean, "true", true)]
    [InlineData(CustomFieldType.Boolean, "sometimes", false)]
    public void Typed_values_are_validated(CustomFieldType type, string value, bool expected)
    {
        var field = new CustomFieldDefinition { Key = "Value", Name = "Value", Type = type };
        Assert.Equal(expected, CustomFieldValueValidator.IsValid(field, value));
    }

    [Fact]
    public void New_context_is_visible_on_all_task_screens_by_default()
    {
        var context = new CustomFieldContext();
        Assert.Equal("Additional information", context.SectionName);
        Assert.True(context.ShowOnCreate);
        Assert.True(context.ShowOnEdit);
        Assert.True(context.ShowOnDetails);
    }
}
