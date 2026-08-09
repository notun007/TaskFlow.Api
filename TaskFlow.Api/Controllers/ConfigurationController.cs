using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Api.Controllers;

public sealed record WorkItemTypeListItem(Guid Id, string Key, string Name, string? Description, bool IsActive, bool IsSystem, int SortOrder, int WorkItems);
public sealed record CreateWorkItemTypeRequest(string Key, string Name, string? Description, int SortOrder);
public sealed record UpdateWorkItemTypeRequest(string Name, string? Description, bool IsActive, int SortOrder);

[ApiController]
[Route("api/configuration/work-item-types")]
[Authorize]
public sealed partial class ConfigurationController(IApplicationDbContext db) : ControllerBase
{
    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{1,29}$")]
    private static partial Regex ValidKey();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkItemTypeListItem>>> List(CancellationToken cancellationToken) =>
        Ok(await Query().ToListAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<WorkItemTypeListItem>> Create(CreateWorkItemTypeRequest request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim();
        var name = request.Name.Trim();
        if (!ValidKey().IsMatch(key)) return BadRequest(new { message = "Key must start with a letter and contain 2-30 letters, numbers, or underscores." });
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Name is required." });
        if (await db.WorkItemTypes.AnyAsync(item => !item.IsDeleted && (item.Key == key || item.Name == name), cancellationToken)) return Conflict(new { message = "A work item type with this key or name already exists." });
        var item = new WorkItemType { Key = key, Name = name, Description = Clean(request.Description), SortOrder = request.SortOrder, IsActive = true };
        db.WorkItemTypes.Add(item);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(WorkItemType), EntityId = item.Id.ToString(), Action = "Created", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(List), await Find(item.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkItemTypeListItem>> Update(Guid id, UpdateWorkItemTypeRequest request, CancellationToken cancellationToken)
    {
        var item = await db.WorkItemTypes.FirstOrDefaultAsync(type => type.Id == id && !type.IsDeleted, cancellationToken);
        if (item is null) return NotFound();
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Name is required." });
        if (await db.WorkItemTypes.AnyAsync(type => type.Id != id && !type.IsDeleted && type.Name == name, cancellationToken)) return Conflict(new { message = "A work item type with this name already exists." });
        item.Name = name;
        item.Description = Clean(request.Description);
        item.IsActive = request.IsActive;
        item.SortOrder = request.SortOrder;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(WorkItemType), EntityId = id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(await Find(id, cancellationToken));
    }

    private IQueryable<WorkItemTypeListItem> Query() => db.WorkItemTypes.AsNoTracking().Where(item => !item.IsDeleted)
        .OrderBy(item => item.SortOrder).ThenBy(item => item.Name)
        .Select(item => new WorkItemTypeListItem(item.Id, item.Key, item.Name, item.Description, item.IsActive, item.IsSystem, item.SortOrder, db.Tasks.Count(task => !task.IsDeleted && task.Type == item.Key)));

    private async Task<WorkItemTypeListItem> Find(Guid id, CancellationToken cancellationToken) => await Query().SingleAsync(item => item.Id == id, cancellationToken);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
