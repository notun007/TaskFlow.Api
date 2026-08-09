using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

public sealed record ReleaseTaskItem(Guid Id, string TaskNumber, string Title, string Type, string Status, string Priority);
public sealed record ReleaseItem(Guid Id, string Name, string? Description, Guid ProjectId, string Status, DateOnly? StartDate, DateOnly? ReleaseDate, DateTimeOffset? ReleasedAt, int TotalTasks, int CompletedTasks, IReadOnlyList<ReleaseTaskItem> Tasks);
public sealed record ReleasePlan(Guid ProjectId, string ProjectName, IReadOnlyList<ReleaseItem> Releases, IReadOnlyList<ReleaseTaskItem> UnassignedTasks);
public sealed record SaveReleaseRequest(string Name, string? Description, Guid ProjectId, DateOnly? StartDate, DateOnly? ReleaseDate);
public sealed record AssignReleaseRequest(Guid? ReleaseId);

[ApiController]
[Route("api/releases")]
[Authorize]
public sealed class ReleasesController(IApplicationDbContext db) : ControllerBase
{
    private static readonly WorkflowStatus[] CompletedStatuses = [WorkflowStatus.Resolved, WorkflowStatus.Closed, WorkflowStatus.Rejected, WorkflowStatus.Cancelled];

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<ReleasePlan>> Plan(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken); if (project is null) return NotFound();
        var releases = await db.ProjectReleases.AsNoTracking().Where(x => x.ProjectId == projectId && !x.IsDeleted).OrderBy(x => x.Status == ReleaseStatus.Unreleased ? 0 : 1).ThenBy(x => x.ReleaseDate).ThenBy(x => x.Name)
            .Select(x => new ReleaseItem(x.Id, x.Name, x.Description, x.ProjectId, x.Status.ToString(), x.StartDate, x.ReleaseDate, x.ReleasedAt, x.Tasks.Count(t => !t.IsDeleted), x.Tasks.Count(t => !t.IsDeleted && CompletedStatuses.Contains(t.Status)), x.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.CreatedAt).Select(t => new ReleaseTaskItem(t.Id, t.TaskNumber, t.Title, t.Type, t.Status.ToString(), t.Priority.ToString())).ToList())).ToListAsync(cancellationToken);
        var unassigned = await db.Tasks.AsNoTracking().Where(x => x.ProjectId == projectId && x.FixVersionId == null && !x.IsDeleted).OrderBy(x => x.CreatedAt).Select(x => new ReleaseTaskItem(x.Id, x.TaskNumber, x.Title, x.Type, x.Status.ToString(), x.Priority.ToString())).ToListAsync(cancellationToken);
        return Ok(new ReleasePlan(project.Id, project.Name, releases, unassigned));
    }

    [HttpPost]
    public async Task<ActionResult<ReleaseItem>> Create(SaveReleaseRequest request, CancellationToken cancellationToken)
    {
        var error = await Validate(request, null, cancellationToken); if (error is not null) return BadRequest(new { message = error });
        var release = new ProjectRelease { Name = request.Name.Trim(), Description = Clean(request.Description), ProjectId = request.ProjectId, StartDate = request.StartDate, ReleaseDate = request.ReleaseDate };
        db.ProjectReleases.Add(release); Audit(release.Id, "Created"); await db.SaveChangesAsync(cancellationToken); return Ok(await Find(release.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReleaseItem>> Update(Guid id, SaveReleaseRequest request, CancellationToken cancellationToken)
    {
        var release = await db.ProjectReleases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken); if (release is null) return NotFound();
        if (release.Status != ReleaseStatus.Unreleased) return Conflict(new { message = "Only an unreleased version can be edited." });
        if (release.ProjectId != request.ProjectId) return BadRequest(new { message = "A release cannot be moved to another project." });
        var error = await Validate(request, id, cancellationToken); if (error is not null) return BadRequest(new { message = error });
        release.Name = request.Name.Trim(); release.Description = Clean(request.Description); release.StartDate = request.StartDate; release.ReleaseDate = request.ReleaseDate; Audit(id, "Updated"); await db.SaveChangesAsync(cancellationToken); return Ok(await Find(id, cancellationToken));
    }

    [HttpPost("{id:guid}/release")]
    public async Task<ActionResult<ReleaseItem>> Release(Guid id, CancellationToken cancellationToken)
    {
        var release = await db.ProjectReleases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken); if (release is null) return NotFound();
        if (release.Status != ReleaseStatus.Unreleased) return Conflict(new { message = "Only an unreleased version can be released." });
        release.Status = ReleaseStatus.Released; release.ReleasedAt = DateTimeOffset.UtcNow; release.ReleaseDate ??= DateOnly.FromDateTime(DateTime.UtcNow); Audit(id, "Released"); await db.SaveChangesAsync(cancellationToken); return Ok(await Find(id, cancellationToken));
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ReleaseItem>> Archive(Guid id, CancellationToken cancellationToken)
    {
        var release = await db.ProjectReleases.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken); if (release is null) return NotFound();
        release.Status = ReleaseStatus.Archived; Audit(id, "Archived"); await db.SaveChangesAsync(cancellationToken); return Ok(await Find(id, cancellationToken));
    }

    [HttpPut("tasks/{taskId:guid}/fix-version")]
    public async Task<IActionResult> Assign(Guid taskId, AssignReleaseRequest request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken); if (task is null) return NotFound();
        if (request.ReleaseId.HasValue)
        {
            var release = await db.ProjectReleases.FirstOrDefaultAsync(x => x.Id == request.ReleaseId && !x.IsDeleted, cancellationToken);
            if (release is null || release.ProjectId != task.ProjectId) return BadRequest(new { message = "The version and task must belong to the same project." });
            if (release.Status != ReleaseStatus.Unreleased) return Conflict(new { message = "Tasks can only be assigned to an unreleased version." });
        }
        task.FixVersionId = request.ReleaseId; db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = task.Id.ToString(), Action = request.ReleaseId.HasValue ? "FixVersionAssigned" : "FixVersionRemoved", ActorReference = User.Identity?.Name ?? "system" }); await db.SaveChangesAsync(cancellationToken); return NoContent();
    }

    private async Task<string?> Validate(SaveReleaseRequest request, Guid? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return "Release name is required.";
        if (!await db.Projects.AnyAsync(x => x.Id == request.ProjectId && !x.IsDeleted, cancellationToken)) return "Select an active project.";
        if (request.StartDate.HasValue && request.ReleaseDate.HasValue && request.ReleaseDate < request.StartDate) return "Release date cannot precede the start date.";
        if (await db.ProjectReleases.AnyAsync(x => x.Id != id && x.ProjectId == request.ProjectId && x.Name == request.Name.Trim() && !x.IsDeleted, cancellationToken)) return "A release with this name already exists in the project.";
        return null;
    }
    private async Task<ReleaseItem> Find(Guid id, CancellationToken cancellationToken) => await db.ProjectReleases.AsNoTracking().Where(x => x.Id == id).Select(x => new ReleaseItem(x.Id, x.Name, x.Description, x.ProjectId, x.Status.ToString(), x.StartDate, x.ReleaseDate, x.ReleasedAt, x.Tasks.Count(t => !t.IsDeleted), x.Tasks.Count(t => !t.IsDeleted && CompletedStatuses.Contains(t.Status)), x.Tasks.Where(t => !t.IsDeleted).Select(t => new ReleaseTaskItem(t.Id, t.TaskNumber, t.Title, t.Type, t.Status.ToString(), t.Priority.ToString())).ToList())).SingleAsync(cancellationToken);
    private void Audit(Guid id, string action) => db.AuditEntries.Add(new AuditEntry { EntityName = nameof(ProjectRelease), EntityId = id.ToString(), Action = action, ActorReference = User.Identity?.Name ?? "system" });
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
