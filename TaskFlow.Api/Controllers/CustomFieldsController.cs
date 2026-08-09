using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Api.Controllers;

public sealed record CustomFieldOptionItem(Guid Id, string Value, string Label, int SortOrder, bool IsActive);
public sealed record CustomFieldContextItem(Guid Id, Guid? WorkItemTypeId, string WorkItemType, bool IsRequired, string? DefaultValue,
    string SectionName, bool ShowOnCreate, bool ShowOnEdit, bool ShowOnDetails);
public sealed record CustomFieldItem(Guid Id, string Key, string Name, string? Description, string Type, bool IsActive, int SortOrder,
    IReadOnlyList<CustomFieldOptionItem> Options, IReadOnlyList<CustomFieldContextItem> Contexts);
public sealed record SaveCustomFieldOptionRequest(string Value, string Label, int SortOrder, bool IsActive);
public sealed record SaveCustomFieldContextRequest(Guid? WorkItemTypeId, bool IsRequired, string? DefaultValue,
    string? SectionName, bool ShowOnCreate, bool ShowOnEdit, bool ShowOnDetails);
public sealed record CreateCustomFieldRequest(string Key, string Name, string? Description, string Type, int SortOrder,
    IReadOnlyList<SaveCustomFieldOptionRequest> Options, IReadOnlyList<SaveCustomFieldContextRequest> Contexts);
public sealed record UpdateCustomFieldRequest(string Name, string? Description, bool IsActive, int SortOrder,
    IReadOnlyList<SaveCustomFieldOptionRequest> Options, IReadOnlyList<SaveCustomFieldContextRequest> Contexts);
public sealed record ApplicableCustomField(Guid Id, string Key, string Name, string? Description, string Type, bool IsRequired,
    string? DefaultValue, string SectionName, bool ShowOnCreate, bool ShowOnEdit, bool ShowOnDetails, IReadOnlyList<CustomFieldOptionItem> Options);

[ApiController]
[Route("api/configuration/custom-fields")]
[Authorize]
public sealed partial class CustomFieldsController(IApplicationDbContext db) : ControllerBase
{
    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{1,49}$")]
    private static partial Regex ValidKey();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomFieldItem>>> List(CancellationToken cancellationToken) =>
        Ok(await Query().ToListAsync(cancellationToken));

    [HttpGet("applicable/{workItemTypeKey}")]
    public async Task<ActionResult<IReadOnlyList<ApplicableCustomField>>> Applicable(string workItemTypeKey, CancellationToken cancellationToken)
    {
        var typeId = await db.WorkItemTypes.Where(x => x.IsActive && x.Key == workItemTypeKey).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken);
        if (typeId is null) return NotFound();
        return Ok(await db.CustomFieldDefinitions.AsNoTracking().Where(x => x.IsActive && x.Contexts.Any(c => !c.IsDeleted && (c.WorkItemTypeId == null || c.WorkItemTypeId == typeId)))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Select(x => new ApplicableCustomField(x.Id, x.Key, x.Name, x.Description, x.Type.ToString(),
                x.Contexts.Where(c => !c.IsDeleted && (c.WorkItemTypeId == typeId || c.WorkItemTypeId == null)).OrderByDescending(c => c.WorkItemTypeId != null).Select(c => c.IsRequired).First(),
                x.Contexts.Where(c => !c.IsDeleted && (c.WorkItemTypeId == typeId || c.WorkItemTypeId == null)).OrderByDescending(c => c.WorkItemTypeId != null).Select(c => c.DefaultValue).First(),
                x.Contexts.Where(c => !c.IsDeleted && (c.WorkItemTypeId == typeId || c.WorkItemTypeId == null)).OrderByDescending(c => c.WorkItemTypeId != null).Select(c => c.SectionName).First(),
                x.Contexts.Where(c => !c.IsDeleted && (c.WorkItemTypeId == typeId || c.WorkItemTypeId == null)).OrderByDescending(c => c.WorkItemTypeId != null).Select(c => c.ShowOnCreate).First(),
                x.Contexts.Where(c => !c.IsDeleted && (c.WorkItemTypeId == typeId || c.WorkItemTypeId == null)).OrderByDescending(c => c.WorkItemTypeId != null).Select(c => c.ShowOnEdit).First(),
                x.Contexts.Where(c => !c.IsDeleted && (c.WorkItemTypeId == typeId || c.WorkItemTypeId == null)).OrderByDescending(c => c.WorkItemTypeId != null).Select(c => c.ShowOnDetails).First(),
                x.Options.Where(o => o.IsActive && !o.IsDeleted).OrderBy(o => o.SortOrder).Select(o => new CustomFieldOptionItem(o.Id, o.Value, o.Label, o.SortOrder, o.IsActive)).ToList()))
            .ToListAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CustomFieldItem>> Create(CreateCustomFieldRequest request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim();
        var name = request.Name.Trim();
        if (!ValidKey().IsMatch(key)) return BadRequest(new { message = "Key must start with a letter and contain 2-50 letters, numbers, or underscores." });
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Name is required." });
        if (!Enum.TryParse<CustomFieldType>(request.Type, true, out var type)) return BadRequest(new { message = "Unsupported custom field type." });
        if (await db.CustomFieldDefinitions.AnyAsync(x => x.Key == key || x.Name == name, cancellationToken)) return Conflict(new { message = "A custom field with this key or name already exists." });
        var validation = await Validate(type, request.Options, request.Contexts, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });

        var field = new CustomFieldDefinition { Key = key, Name = name, Description = Clean(request.Description), Type = type, SortOrder = request.SortOrder };
        db.CustomFieldDefinitions.Add(field);
        SyncChildren(field, request.Options, request.Contexts);
        Audit(field.Id, "Created");
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(List), await Find(field.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomFieldItem>> Update(Guid id, UpdateCustomFieldRequest request, CancellationToken cancellationToken)
    {
        var field = await db.CustomFieldDefinitions.Include(x => x.Options).Include(x => x.Contexts).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (field is null) return NotFound();
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Name is required." });
        if (await db.CustomFieldDefinitions.AnyAsync(x => x.Id != id && x.Name == name, cancellationToken)) return Conflict(new { message = "A custom field with this name already exists." });
        var validation = await Validate(field.Type, request.Options, request.Contexts, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });

        field.Name = name;
        field.Description = Clean(request.Description);
        field.IsActive = request.IsActive;
        field.SortOrder = request.SortOrder;
        SyncChildren(field, request.Options, request.Contexts);
        Audit(field.Id, "Updated");
        await db.SaveChangesAsync(cancellationToken);
        return Ok(await Find(id, cancellationToken));
    }

    private async Task<string?> Validate(CustomFieldType type, IReadOnlyList<SaveCustomFieldOptionRequest>? options,
        IReadOnlyList<SaveCustomFieldContextRequest>? contexts, CancellationToken cancellationToken)
    {
        options ??= [];
        contexts ??= [];
        if (type is CustomFieldType.Select or CustomFieldType.MultiSelect && options.Count == 0) return "Select fields require at least one option.";
        if (options.Any(x => string.IsNullOrWhiteSpace(x.Value) || string.IsNullOrWhiteSpace(x.Label))) return "Every option requires a value and label.";
        if (options.GroupBy(x => x.Value.Trim(), StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1)) return "Option values must be unique.";
        if (contexts.GroupBy(x => x.WorkItemTypeId).Any(x => x.Count() > 1)) return "A work item type can only appear once in the field context.";
        var ids = contexts.Where(x => x.WorkItemTypeId.HasValue).Select(x => x.WorkItemTypeId!.Value).Distinct().ToArray();
        if (ids.Length > 0 && await db.WorkItemTypes.CountAsync(x => ids.Contains(x.Id) && x.IsActive, cancellationToken) != ids.Length) return "One or more selected work item types are unavailable.";
        return null;
    }

    private void SyncChildren(CustomFieldDefinition field, IReadOnlyList<SaveCustomFieldOptionRequest>? optionRequests,
        IReadOnlyList<SaveCustomFieldContextRequest>? contextRequests)
    {
        optionRequests ??= [];
        contextRequests = contextRequests is { Count: > 0 } ? contextRequests : [new(null, false, null, "Additional information", true, true, true)];
        foreach (var option in field.Options) option.IsDeleted = true;
        foreach (var request in optionRequests)
        {
            var value = request.Value.Trim();
            var option = field.Options.FirstOrDefault(x => string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
            if (option is null) field.Options.Add(new CustomFieldOption { Value = value, Label = request.Label.Trim(), SortOrder = request.SortOrder, IsActive = request.IsActive });
            else { option.IsDeleted = false; option.Label = request.Label.Trim(); option.SortOrder = request.SortOrder; option.IsActive = request.IsActive; }
        }
        foreach (var context in field.Contexts) context.IsDeleted = true;
        foreach (var request in contextRequests)
        {
            var context = field.Contexts.FirstOrDefault(x => x.WorkItemTypeId == request.WorkItemTypeId);
            if (context is null) field.Contexts.Add(new CustomFieldContext { WorkItemTypeId = request.WorkItemTypeId, IsRequired = request.IsRequired, DefaultValue = Clean(request.DefaultValue), SectionName = Clean(request.SectionName) ?? "Additional information", ShowOnCreate = request.ShowOnCreate, ShowOnEdit = request.ShowOnEdit, ShowOnDetails = request.ShowOnDetails });
            else { context.IsDeleted = false; context.IsRequired = request.IsRequired; context.DefaultValue = Clean(request.DefaultValue); context.SectionName = Clean(request.SectionName) ?? "Additional information"; context.ShowOnCreate = request.ShowOnCreate; context.ShowOnEdit = request.ShowOnEdit; context.ShowOnDetails = request.ShowOnDetails; }
        }
    }

    private IQueryable<CustomFieldItem> Query() => db.CustomFieldDefinitions.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
        .Select(x => new CustomFieldItem(x.Id, x.Key, x.Name, x.Description, x.Type.ToString(), x.IsActive, x.SortOrder,
            x.Options.Where(o => !o.IsDeleted).OrderBy(o => o.SortOrder).Select(o => new CustomFieldOptionItem(o.Id, o.Value, o.Label, o.SortOrder, o.IsActive)).ToList(),
            x.Contexts.Where(c => !c.IsDeleted).Select(c => new CustomFieldContextItem(c.Id, c.WorkItemTypeId, c.WorkItemTypeId == null ? "All work item types" : c.WorkItemType!.Name, c.IsRequired, c.DefaultValue, c.SectionName, c.ShowOnCreate, c.ShowOnEdit, c.ShowOnDetails)).ToList()));

    private async Task<CustomFieldItem> Find(Guid id, CancellationToken cancellationToken) => await Query().SingleAsync(x => x.Id == id, cancellationToken);
    private void Audit(Guid id, string action) => db.AuditEntries.Add(new AuditEntry { EntityName = nameof(CustomFieldDefinition), EntityId = id.ToString(), Action = action, ActorReference = User.Identity?.Name ?? "system" });
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
