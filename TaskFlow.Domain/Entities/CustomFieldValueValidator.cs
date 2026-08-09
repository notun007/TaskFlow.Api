using System.Globalization;
using System.Text.Json;

namespace TaskFlow.Domain.Entities;

public static class CustomFieldValueValidator
{
    public static bool IsValid(CustomFieldDefinition field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        value = value.Trim();
        if (field.Type == CustomFieldType.Number && !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) return false;
        if (field.Type == CustomFieldType.Date && !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) return false;
        if (field.Type == CustomFieldType.Boolean && !bool.TryParse(value, out _)) return false;
        var allowed = field.Options.Where(x => x.IsActive && !x.IsDeleted).Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (field.Type == CustomFieldType.Select) return allowed.Contains(value);
        if (field.Type != CustomFieldType.MultiSelect) return true;
        try { return JsonSerializer.Deserialize<string[]>(value)?.All(allowed.Contains) == true; }
        catch (JsonException) { return false; }
    }
}
