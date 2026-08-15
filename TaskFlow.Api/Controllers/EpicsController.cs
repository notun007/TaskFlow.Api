using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Identity;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

public sealed record EpicListItem(Guid Id, string Name, string? Description, Guid ProjectId, string ProjectName, string Status, DateOnly? TargetDate, int TotalItems, int CompletedItems, int ProgressPercent, DateTimeOffset CreatedAt);
public sealed record SaveEpicRequest(string Name, string? Description, Guid ProjectId, DateOnly? TargetDate);
public sealed record ChangeEpicStatusRequest(EpicStatus Status);

[ApiController]
[Route("api/epics")]
[Authorize]
public sealed class EpicsController(IApplicationDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    private static readonly WorkflowStatus[] CompletedTaskStatuses = [WorkflowStatus.Resolved, WorkflowStatus.Closed, WorkflowStatus.Rejected, WorkflowStatus.Cancelled];
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EpicListItem>>> List([FromQuery] Guid? projectId, CancellationToken cancellationToken)
        => Ok(await LoadItems(projectId, null, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EpicListItem>> Get(Guid id, CancellationToken cancellationToken) =>
        (await LoadItems(null, id, cancellationToken)).SingleOrDefault() is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<ActionResult<EpicListItem>> Create(SaveEpicRequest request, CancellationToken cancellationToken)
    {
        if (!await CanManage(request.ProjectId, cancellationToken)) return Forbid();
        var validation = await Validate(request, null, cancellationToken); if (validation is not null) return validation;
        var epic = new Epic { Name = request.Name.Trim(), Description = Clean(request.Description), ProjectId = request.ProjectId, TargetDate = request.TargetDate };
        db.Epics.Add(epic); Audit(epic, "Created"); await db.SaveChangesAsync(cancellationToken);
        return Ok((await LoadItems(null, epic.Id, cancellationToken)).Single());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EpicListItem>> Update(Guid id, SaveEpicRequest request, CancellationToken cancellationToken)
    {
        var epic = await db.Epics.FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (epic is null) return NotFound();
        if (!await CanManage(epic.ProjectId, cancellationToken)) return Forbid();
        if (request.ProjectId != epic.ProjectId) return BadRequest(new { message = "An Epic cannot be moved to another project." });
        var validation = await Validate(request, id, cancellationToken); if (validation is not null) return validation;
        epic.Name = request.Name.Trim(); epic.Description = Clean(request.Description); epic.TargetDate = request.TargetDate; Audit(epic, "Updated"); await db.SaveChangesAsync(cancellationToken);
        return Ok((await LoadItems(null, epic.Id, cancellationToken)).Single());
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<EpicListItem>> ChangeStatus(Guid id, ChangeEpicStatusRequest request, CancellationToken cancellationToken)
    {
        var epic = await db.Epics.FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (epic is null) return NotFound();
        if (!await CanManage(epic.ProjectId, cancellationToken)) return Forbid();
        epic.Status = request.Status; Audit(epic, $"StatusChanged:{request.Status}"); await db.SaveChangesAsync(cancellationToken);
        return Ok((await LoadItems(null, epic.Id, cancellationToken)).Single());
    }

    private async Task<List<EpicListItem>> LoadItems(Guid? projectId, Guid? epicId, CancellationToken cancellationToken)
    {
        var query = db.Epics.AsNoTracking().Where(x => !x.IsDeleted);
        if (projectId.HasValue) query = query.Where(x => x.ProjectId == projectId.Value);
        if (epicId.HasValue) query = query.Where(x => x.Id == epicId.Value);
        var epics = await query.ToListAsync(cancellationToken);
        if (epics.Count == 0) return [];

        // Keep Oracle queries deliberately simple. Converted enum/date fields, navigation joins,
        // and aggregate projections are assembled below after materialization.
        var projectIds = epics.Select(x => x.ProjectId).Distinct().ToArray();
        var projectNames = await db.Projects.AsNoTracking()
            .Where(x => projectIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        var epicIds = epics.Select(x => x.Id).ToHashSet();
        var childTasks = await db.Tasks.AsNoTracking()
            .Where(x => x.EpicId != null)
            .Select(x => new { x.EpicId, x.Status })
            .ToListAsync(cancellationToken);
        var counts = childTasks.Where(x => x.EpicId.HasValue && epicIds.Contains(x.EpicId.Value))
            .GroupBy(x => x.EpicId!.Value)
            .ToDictionary(x => x.Key, x => new { Total = x.Count(), Completed = x.Count(task => CompletedTaskStatuses.Contains(task.Status)) });

        return epics.Select(epic =>
        {
            var count = counts.GetValueOrDefault(epic.Id);
            var total = count?.Total ?? 0;
            var completed = count?.Completed ?? 0;
            return new EpicListItem(epic.Id, epic.Name, epic.Description, epic.ProjectId,
                projectNames.GetValueOrDefault(epic.ProjectId, "Unknown project"), epic.Status.ToString(), epic.TargetDate,
                total, completed, total == 0 ? 0 : completed * 100 / total, epic.CreatedAt);
        }).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Name).ToList();
    }

    private async Task<ActionResult?> Validate(SaveEpicRequest request, Guid? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Epic name is required." });
        if (!await db.Projects.AnyAsync(x => x.Id == request.ProjectId && !x.IsDeleted, cancellationToken)) return BadRequest(new { message = "Select an active project." });
        if (await db.Epics.AnyAsync(x => x.ProjectId == request.ProjectId && x.Name == request.Name.Trim() && x.Id != id, cancellationToken)) return Conflict(new { message = "An Epic with this name already exists in the project." });
        return null;
    }

    private async Task<bool> CanManage(Guid projectId, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(User.Identity?.Name ?? string.Empty); if (user is null || !user.IsActive) return false;
        if (await users.IsInRoleAsync(user, "Administrator")) return true;
        return await db.ProjectRoleAssignments.AnyAsync(x => x.ProjectId == projectId && x.UserId == user.Id && (x.Role == ProjectRole.ProjectAdmin || x.Role == ProjectRole.ProductOwner), cancellationToken);
    }

    private void Audit(Epic epic, string action) => db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Epic), EntityId = epic.Id.ToString(), Action = action, ActorReference = User.Identity?.Name ?? "system", ChangesJson = JsonSerializer.Serialize(new { epic.ProjectId, epic.Name, epic.Status, epic.TargetDate }) });
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
