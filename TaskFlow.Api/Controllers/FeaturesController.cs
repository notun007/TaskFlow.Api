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

public sealed record FeatureListItem(Guid Id, string Name, string? Description, Guid ProjectId, string ProjectName, Guid EpicId, string EpicName, string Status, DateOnly? TargetDate, int TotalItems, int CompletedItems, int ProgressPercent, DateTimeOffset CreatedAt);
public sealed record SaveFeatureRequest(string Name, string? Description, Guid ProjectId, Guid EpicId, DateOnly? TargetDate);
public sealed record ChangeFeatureStatusRequest(FeatureStatus Status);

[ApiController]
[Route("api/features")]
[Authorize]
public sealed class FeaturesController(IApplicationDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    private static readonly WorkflowStatus[] CompletedTaskStatuses = [WorkflowStatus.Resolved, WorkflowStatus.Closed, WorkflowStatus.Rejected, WorkflowStatus.Cancelled];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeatureListItem>>> List([FromQuery] Guid? projectId, [FromQuery] Guid? epicId, CancellationToken cancellationToken) =>
        Ok(await LoadItems(projectId, epicId, null, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeatureListItem>> Get(Guid id, CancellationToken cancellationToken) =>
        (await LoadItems(null, null, id, cancellationToken)).SingleOrDefault() is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public async Task<ActionResult<FeatureListItem>> Create(SaveFeatureRequest request, CancellationToken cancellationToken)
    {
        if (!await CanManage(request.ProjectId, cancellationToken)) return Forbid();
        var validation = await Validate(request, null, cancellationToken); if (validation is not null) return validation;
        var feature = new Feature { Name = request.Name.Trim(), Description = Clean(request.Description), ProjectId = request.ProjectId, EpicId = request.EpicId, TargetDate = request.TargetDate };
        db.Features.Add(feature); Audit(feature, "Created"); await db.SaveChangesAsync(cancellationToken);
        return Ok((await LoadItems(null, null, feature.Id, cancellationToken)).Single());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeatureListItem>> Update(Guid id, SaveFeatureRequest request, CancellationToken cancellationToken)
    {
        var feature = await db.Features.FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (feature is null) return NotFound();
        if (!await CanManage(feature.ProjectId, cancellationToken)) return Forbid();
        if (request.ProjectId != feature.ProjectId) return BadRequest(new { message = "A Feature cannot be moved to another project." });
        var validation = await Validate(request, id, cancellationToken); if (validation is not null) return validation;
        if (feature.EpicId != request.EpicId)
        {
            var childTasks = await db.Tasks.Where(x => x.FeatureId == feature.Id).ToListAsync(cancellationToken);
            foreach (var task in childTasks) task.EpicId = request.EpicId;
        }
        feature.Name = request.Name.Trim(); feature.Description = Clean(request.Description); feature.EpicId = request.EpicId; feature.TargetDate = request.TargetDate;
        Audit(feature, "Updated"); await db.SaveChangesAsync(cancellationToken);
        return Ok((await LoadItems(null, null, feature.Id, cancellationToken)).Single());
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<FeatureListItem>> ChangeStatus(Guid id, ChangeFeatureStatusRequest request, CancellationToken cancellationToken)
    {
        var feature = await db.Features.FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (feature is null) return NotFound();
        if (!await CanManage(feature.ProjectId, cancellationToken)) return Forbid();
        feature.Status = request.Status; Audit(feature, $"StatusChanged:{request.Status}"); await db.SaveChangesAsync(cancellationToken);
        return Ok((await LoadItems(null, null, feature.Id, cancellationToken)).Single());
    }

    private async Task<List<FeatureListItem>> LoadItems(Guid? projectId, Guid? epicId, Guid? featureId, CancellationToken cancellationToken)
    {
        var query = db.Features.AsNoTracking();
        if (projectId.HasValue) query = query.Where(x => x.ProjectId == projectId.Value);
        if (epicId.HasValue) query = query.Where(x => x.EpicId == epicId.Value);
        if (featureId.HasValue) query = query.Where(x => x.Id == featureId.Value);
        var features = await query.ToListAsync(cancellationToken); if (features.Count == 0) return [];
        var projectIds = features.Select(x => x.ProjectId).Distinct().ToArray();
        var epicIds = features.Select(x => x.EpicId).Distinct().ToArray();
        var projects = await db.Projects.AsNoTracking().Where(x => projectIds.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        var epics = await db.Epics.AsNoTracking().Where(x => epicIds.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        var featureIds = features.Select(x => x.Id).ToHashSet();
        var childTasks = await db.Tasks.AsNoTracking().Where(x => x.FeatureId != null).Select(x => new { x.FeatureId, x.Status }).ToListAsync(cancellationToken);
        var counts = childTasks.Where(x => x.FeatureId.HasValue && featureIds.Contains(x.FeatureId.Value)).GroupBy(x => x.FeatureId!.Value)
            .ToDictionary(x => x.Key, x => new { Total = x.Count(), Completed = x.Count(task => CompletedTaskStatuses.Contains(task.Status)) });
        return features.Select(feature => { var count = counts.GetValueOrDefault(feature.Id); var total = count?.Total ?? 0; var completed = count?.Completed ?? 0;
            return new FeatureListItem(feature.Id, feature.Name, feature.Description, feature.ProjectId, projects.GetValueOrDefault(feature.ProjectId, "Unknown project"), feature.EpicId, epics.GetValueOrDefault(feature.EpicId, "Unknown Epic"), feature.Status.ToString(), feature.TargetDate, total, completed, total == 0 ? 0 : completed * 100 / total, feature.CreatedAt);
        }).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Name).ToList();
    }

    private async Task<ActionResult?> Validate(SaveFeatureRequest request, Guid? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Feature name is required." });
        if (!await db.Epics.AnyAsync(x => x.Id == request.EpicId && x.ProjectId == request.ProjectId && x.Status == EpicStatus.Active, cancellationToken)) return BadRequest(new { message = "Select an active Epic belonging to this project." });
        if (await db.Features.AnyAsync(x => x.EpicId == request.EpicId && x.Name == request.Name.Trim() && x.Id != id, cancellationToken)) return Conflict(new { message = "A Feature with this name already exists in the Epic." });
        return null;
    }

    private async Task<bool> CanManage(Guid projectId, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(User.Identity?.Name ?? string.Empty); if (user is null || !user.IsActive) return false;
        if (await users.IsInRoleAsync(user, "Administrator")) return true;
        return await db.ProjectRoleAssignments.AnyAsync(x => x.ProjectId == projectId && x.UserId == user.Id && (x.Role == ProjectRole.ProjectAdmin || x.Role == ProjectRole.ProductOwner), cancellationToken);
    }

    private void Audit(Feature feature, string action) => db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Feature), EntityId = feature.Id.ToString(), Action = action, ActorReference = User.Identity?.Name ?? "system", ChangesJson = JsonSerializer.Serialize(new { feature.ProjectId, feature.EpicId, feature.Name, feature.Status, feature.TargetDate }) });
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
