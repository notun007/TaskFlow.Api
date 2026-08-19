using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

public sealed record SprintTaskItem(Guid Id, string TaskNumber, string Title, string Type, string Status, string Priority, DateTimeOffset? DueDate, Guid? EpicId, string? EpicName, Guid? FeatureId, string? FeatureName,
    Guid? OwnerUserId, Guid? ReporterUserId, bool ReportedByMe, bool OwnedByMe, bool TestingByMe, bool UatByMe);
public sealed record SprintItem(Guid Id, string Name, string? Goal, Guid ProjectId, string Status, DateOnly? StartDate, DateOnly? EndDate, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt, IReadOnlyList<SprintTaskItem> Tasks);
public sealed record BacklogDetails(Guid ProjectId, string ProjectName, IReadOnlyList<SprintItem> Sprints, IReadOnlyList<SprintTaskItem> Backlog);
public sealed record SaveSprintRequest(string Name, string? Goal, Guid ProjectId, DateOnly? StartDate, DateOnly? EndDate);
public sealed record AssignSprintRequest(Guid? SprintId);

[ApiController]
[Route("api/sprints")]
[Authorize]
public sealed class SprintsController(IApplicationDbContext db) : ControllerBase
{
    private static readonly WorkflowStatus[] CompletedStatuses = [WorkflowStatus.Resolved, WorkflowStatus.Closed, WorkflowStatus.Rejected, WorkflowStatus.Cancelled];

    [HttpGet("backlog/{projectId:guid}")]
    public async Task<ActionResult<BacklogDetails>> Backlog(Guid projectId, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUserId();
        var currentUserReference = User.Identity?.Name ?? string.Empty;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken);
        if (project is null) return NotFound();
        await ReconcileTaskStatuses(projectId, cancellationToken);
        var sprints = await db.Sprints.AsNoTracking().Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SprintItem(x.Id, x.Name, x.Goal, x.ProjectId, x.Status.ToString(), x.StartDate, x.EndDate, x.StartedAt, x.CompletedAt, x.CreatedAt,
                x.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.CreatedAt).Select(t => new SprintTaskItem(t.Id, t.TaskNumber, t.Title, t.Type, t.Status.ToString(), t.Priority.ToString(), t.DueDate, t.EpicId, t.Epic != null ? t.Epic.Name : null, t.FeatureId, t.Feature != null ? t.Feature.Name : null,
                    t.OwnerUserId, t.ReporterUserId, currentUserId.HasValue && t.ReporterUserId == currentUserId, currentUserId.HasValue && t.OwnerUserId == currentUserId,
                    t.Assignments.Any(a => !a.IsDeleted && a.Responsibility == ResponsibilityType.Tester && a.PartyReference == currentUserReference),
                    t.Assignments.Any(a => !a.IsDeleted && a.Responsibility == ResponsibilityType.UatOwner && a.PartyReference == currentUserReference))).ToList())).ToListAsync(cancellationToken);
        var backlog = await db.Tasks.AsNoTracking().Where(x => x.ProjectId == projectId && x.SprintId == null && !x.IsDeleted).OrderBy(x => x.CreatedAt).Select(x => new SprintTaskItem(x.Id, x.TaskNumber, x.Title, x.Type, x.Status.ToString(), x.Priority.ToString(), x.DueDate, x.EpicId, x.Epic != null ? x.Epic.Name : null, x.FeatureId, x.Feature != null ? x.Feature.Name : null,
            x.OwnerUserId, x.ReporterUserId, currentUserId.HasValue && x.ReporterUserId == currentUserId, currentUserId.HasValue && x.OwnerUserId == currentUserId,
            x.Assignments.Any(a => !a.IsDeleted && a.Responsibility == ResponsibilityType.Tester && a.PartyReference == currentUserReference),
            x.Assignments.Any(a => !a.IsDeleted && a.Responsibility == ResponsibilityType.UatOwner && a.PartyReference == currentUserReference))).ToListAsync(cancellationToken);
        return Ok(new BacklogDetails(project.Id, project.Name, sprints, backlog));
    }

    private async Task ReconcileTaskStatuses(Guid projectId, CancellationToken cancellationToken)
    {
        var schemeId = await db.WorkflowSchemes.Where(x => x.ProjectId == projectId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? await db.WorkflowSchemes.Where(x => x.IsDefault).Select(x => (Guid?)x.Id).SingleAsync(cancellationToken);
        var transitions = await db.WorkflowTransitions.AsNoTracking().Where(x => x.WorkflowSchemeId == schemeId)
            .OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        if (transitions.Count == 0) return;
        var validStatuses = transitions.SelectMany(x => new[] { x.FromStatus, x.ToStatus }).ToHashSet();
        var entryStatus = transitions[0].FromStatus;
        var tasks = await db.Tasks.Where(x => x.ProjectId == projectId && !x.IsDeleted
            && (!validStatuses.Contains(x.Status)
                || (entryStatus != WorkflowStatus.Submitted
                    && x.Status == WorkflowStatus.Submitted
                    && !db.TaskStatusHistory.Any(history => history.TaskItemId == x.Id))))
            .ToListAsync(cancellationToken);
        if (tasks.Count == 0) return;
        var actor = User.Identity?.Name ?? "system";
        foreach (var task in tasks)
        {
            var previousStatus = task.Status;
            task.Status = entryStatus;
            task.UpdatedAt = DateTimeOffset.UtcNow;
            db.TaskStatusHistory.Add(new TaskStatusHistory { TaskItemId = task.Id, FromStatus = previousStatus, ToStatus = entryStatus, ActorReference = actor, Comment = validStatuses.Contains(previousStatus)
                ? "Moved to the workflow entry stage because this task was created before workflow-based initial statuses were applied."
                : "Moved to the workflow entry stage because the previous status is not part of the project workflow." });
            db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = task.Id.ToString(), Action = $"WorkflowStatusReconciled:{entryStatus}", ActorReference = actor, ChangesJson = JsonSerializer.Serialize(new { fromStatus = previousStatus, toStatus = entryStatus }) });
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<SprintItem>> Create(SaveSprintRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Sprint name is required." });
        if (!await db.Projects.AnyAsync(x => x.Id == request.ProjectId && !x.IsDeleted, cancellationToken)) return BadRequest(new { message = "Select an active project." });
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate) return BadRequest(new { message = "Sprint end date cannot precede its start date." });
        if (await db.Sprints.AnyAsync(x => x.ProjectId == request.ProjectId && x.Name == name && !x.IsDeleted, cancellationToken)) return Conflict(new { message = "A sprint with this name already exists in the project." });
        var sprint = new Sprint { Name = name, Goal = Clean(request.Goal), ProjectId = request.ProjectId, StartDate = request.StartDate, EndDate = request.EndDate };
        db.Sprints.Add(sprint); Audit(sprint.Id, "Created"); await db.SaveChangesAsync(cancellationToken);
        return Ok(await Find(sprint.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SprintItem>> Update(Guid id, SaveSprintRequest request, CancellationToken cancellationToken)
    {
        var sprint = await db.Sprints.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken); if (sprint is null) return NotFound();
        if (sprint.Status == SprintStatus.Completed) return Conflict(new { message = "A completed sprint cannot be edited." });
        var name = request.Name.Trim(); if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Sprint name is required." });
        if (request.ProjectId != sprint.ProjectId) return BadRequest(new { message = "A sprint cannot be moved to another project." });
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate) return BadRequest(new { message = "Sprint end date cannot precede its start date." });
        sprint.Name = name; sprint.Goal = Clean(request.Goal); sprint.StartDate = request.StartDate; sprint.EndDate = request.EndDate; Audit(id, "Updated"); await db.SaveChangesAsync(cancellationToken);
        return Ok(await Find(id, cancellationToken));
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<SprintItem>> Start(Guid id, CancellationToken cancellationToken)
    {
        var sprint = await db.Sprints.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken); if (sprint is null) return NotFound();
        if (sprint.Status != SprintStatus.Planned) return Conflict(new { message = "Only a planned sprint can be started." });
        if (await db.Sprints.AnyAsync(x => x.ProjectId == sprint.ProjectId && x.Status == SprintStatus.Active && !x.IsDeleted, cancellationToken)) return Conflict(new { message = "This project already has an active sprint." });
        sprint.Status = SprintStatus.Active; sprint.StartedAt = DateTimeOffset.UtcNow; sprint.StartDate ??= DateOnly.FromDateTime(DateTime.UtcNow); Audit(id, "Started"); await db.SaveChangesAsync(cancellationToken);
        return Ok(await Find(id, cancellationToken));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<SprintItem>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var sprint = await db.Sprints.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken); if (sprint is null) return NotFound();
        if (sprint.Status != SprintStatus.Active) return Conflict(new { message = "Only an active sprint can be completed." });
        foreach (var task in sprint.Tasks.Where(x => !x.IsDeleted && !CompletedStatuses.Contains(x.Status))) task.SprintId = null;
        sprint.Status = SprintStatus.Completed; sprint.CompletedAt = DateTimeOffset.UtcNow; sprint.EndDate ??= DateOnly.FromDateTime(DateTime.UtcNow); Audit(id, "Completed"); await db.SaveChangesAsync(cancellationToken);
        return Ok(await Find(id, cancellationToken));
    }

    [HttpPut("tasks/{taskId:guid}/sprint")]
    public async Task<IActionResult> Assign(Guid taskId, AssignSprintRequest request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken); if (task is null) return NotFound();
        if (request.SprintId.HasValue)
        {
            var sprint = await db.Sprints.FirstOrDefaultAsync(x => x.Id == request.SprintId && !x.IsDeleted, cancellationToken);
            if (sprint is null || sprint.ProjectId != task.ProjectId) return BadRequest(new { message = "The sprint and task must belong to the same project." });
            if (sprint.Status == SprintStatus.Completed) return Conflict(new { message = "Tasks cannot be assigned to a completed sprint." });
        }
        task.SprintId = request.SprintId; db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = taskId.ToString(), Action = request.SprintId.HasValue ? "SprintAssigned" : "MovedToBacklog", ActorReference = User.Identity?.Name ?? "system" }); await db.SaveChangesAsync(cancellationToken); return NoContent();
    }

    private async Task<SprintItem> Find(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUserId();
        var currentUserReference = User.Identity?.Name ?? string.Empty;
        return await db.Sprints.AsNoTracking().Where(x => x.Id == id).Select(x => new SprintItem(x.Id, x.Name, x.Goal, x.ProjectId, x.Status.ToString(), x.StartDate, x.EndDate, x.StartedAt, x.CompletedAt, x.CreatedAt, x.Tasks.Where(t => !t.IsDeleted).Select(t => new SprintTaskItem(t.Id, t.TaskNumber, t.Title, t.Type, t.Status.ToString(), t.Priority.ToString(), t.DueDate, t.EpicId, t.Epic != null ? t.Epic.Name : null, t.FeatureId, t.Feature != null ? t.Feature.Name : null,
            t.OwnerUserId, t.ReporterUserId, currentUserId.HasValue && t.ReporterUserId == currentUserId, currentUserId.HasValue && t.OwnerUserId == currentUserId,
            t.Assignments.Any(a => !a.IsDeleted && a.Responsibility == ResponsibilityType.Tester && a.PartyReference == currentUserReference),
            t.Assignments.Any(a => !a.IsDeleted && a.Responsibility == ResponsibilityType.UatOwner && a.PartyReference == currentUserReference))).ToList())).SingleAsync(cancellationToken);
    }
    private void Audit(Guid id, string action) => db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Sprint), EntityId = id.ToString(), Action = action, ActorReference = User.Identity?.Name ?? "system" });
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private Guid? CurrentUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
