using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

public sealed record WorkflowTransitionItem(Guid Id, string FromStatus, string ToStatus, int SortOrder);
public sealed record WorkflowDetails(Guid Id, string Name, Guid? ProjectId, string Project, bool IsDefault, bool IsInherited,
    IReadOnlyList<string> Statuses, IReadOnlyList<WorkflowTransitionItem> Transitions);
public sealed record SaveWorkflowTransitionRequest(string FromStatus, string ToStatus, int SortOrder);
public sealed record SaveWorkflowRequest(string Name, IReadOnlyList<SaveWorkflowTransitionRequest> Transitions);

[ApiController]
[Route("api/configuration/workflows")]
[Authorize]
public sealed class WorkflowsController(IApplicationDbContext db, ILogger<WorkflowsController> logger) : ControllerBase
{
    [HttpPost("{projectId:guid}/reconcile-tasks")]
    public async Task<ActionResult<object>> ReconcileTasks(Guid projectId, CancellationToken cancellationToken)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken)) return NotFound();
        var schemeId = await db.WorkflowSchemes.Where(x => x.ProjectId == projectId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? await db.WorkflowSchemes.Where(x => x.IsDefault).Select(x => (Guid?)x.Id).SingleAsync(cancellationToken);
        var transitions = await db.WorkflowTransitions.AsNoTracking().Where(x => x.WorkflowSchemeId == schemeId)
            .OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        if (transitions.Count == 0) return Conflict(new { message = "The project workflow has no transitions." });

        var validStatuses = transitions.SelectMany(x => new[] { x.FromStatus, x.ToStatus }).ToHashSet();
        var entryStatus = transitions[0].FromStatus;
        var tasks = await db.Tasks.Where(x => x.ProjectId == projectId && !x.IsDeleted && !validStatuses.Contains(x.Status)).ToListAsync(cancellationToken);
        var actor = User.Identity?.Name ?? "system";
        foreach (var task in tasks)
        {
            var previousStatus = task.Status;
            task.Status = entryStatus;
            task.UpdatedAt = DateTimeOffset.UtcNow;
            db.TaskStatusHistory.Add(new TaskStatusHistory { TaskItemId = task.Id, FromStatus = previousStatus, ToStatus = entryStatus, ActorReference = actor, Comment = "Moved to the workflow entry stage because the previous status is not part of the project workflow." });
            db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = task.Id.ToString(), Action = $"WorkflowStatusReconciled:{entryStatus}", ActorReference = actor, ChangesJson = JsonSerializer.Serialize(new { fromStatus = previousStatus, toStatus = entryStatus }) });
        }
        if (tasks.Count > 0) await db.SaveChangesAsync(cancellationToken);
        return Ok(new { reconciledCount = tasks.Count, entryStatus = entryStatus.ToString() });
    }

    [HttpGet]
    public async Task<ActionResult<WorkflowDetails>> Get([FromQuery] Guid? projectId, CancellationToken cancellationToken)
    {
        if (projectId.HasValue && !await db.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken)) return NotFound();
        var scheme = projectId.HasValue
            ? await db.WorkflowSchemes.AsNoTracking().Include(x => x.Project).Include(x => x.Transitions).SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken)
            : await db.WorkflowSchemes.AsNoTracking().Include(x => x.Transitions).SingleAsync(x => x.IsDefault, cancellationToken);
        var inherited = scheme is null || (projectId.HasValue && scheme.Transitions.Count == 0);
        if (inherited)
            scheme = await db.WorkflowSchemes.AsNoTracking().Include(x => x.Transitions).SingleAsync(x => x.IsDefault, cancellationToken);
        if (scheme is null) return NotFound();
        var corruptedDefault = scheme.IsDefault && scheme.Transitions.Count == 3
            && scheme.Transitions.Any(x => x.FromStatus == WorkflowStatus.Submitted && x.ToStatus == WorkflowStatus.Triaged)
            && scheme.Transitions.Any(x => x.FromStatus == WorkflowStatus.Triaged && x.ToStatus == WorkflowStatus.InProgress)
            && scheme.Transitions.Any(x => x.FromStatus == WorkflowStatus.InProgress && x.ToStatus == WorkflowStatus.Resolved);
        if ((scheme.Transitions.Count == 0 || corruptedDefault) && scheme.IsDefault)
        {
            if (corruptedDefault)
                db.WorkflowTransitions.RemoveRange(scheme.Transitions);
            foreach (var item in BuiltInWorkflow.Transitions)
                db.WorkflowTransitions.Add(new WorkflowTransition { WorkflowSchemeId = scheme.Id, FromStatus = item.From, ToStatus = item.To, SortOrder = item.SortOrder });
            await db.SaveChangesAsync(cancellationToken);
            scheme = await db.WorkflowSchemes.AsNoTracking().Include(x => x.Transitions).SingleAsync(x => x.IsDefault, cancellationToken);
        }
        return Ok(ToDetails(scheme, projectId, inherited));
    }

    [HttpPut("{projectId:guid?}")]
    public async Task<ActionResult<WorkflowDetails>> Save(Guid? projectId, SaveWorkflowRequest request, CancellationToken cancellationToken)
    {
        if (projectId.HasValue && !await db.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken)) return NotFound();
        var parsed = new List<(WorkflowStatus From, WorkflowStatus To, int SortOrder)>();
        foreach (var transition in request.Transitions ?? [])
        {
            if (!Enum.TryParse<WorkflowStatus>(transition.FromStatus, true, out var from) || !Enum.TryParse<WorkflowStatus>(transition.ToStatus, true, out var to)) return BadRequest(new { message = "A transition contains an unsupported status." });
            if (from == to) return BadRequest(new { message = "A workflow cannot transition a status to itself." });
            parsed.Add((from, to, transition.SortOrder));
        }
        if (parsed.Count == 0) return BadRequest(new { message = "Add at least one workflow transition." });
        if (parsed.GroupBy(x => new { x.From, x.To }).Any(x => x.Count() > 1)) return BadRequest(new { message = "Duplicate workflow transitions are not allowed." });

        var scheme = projectId.HasValue
            ? await db.WorkflowSchemes.Include(x => x.Project).SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken)
            : await db.WorkflowSchemes.SingleAsync(x => x.IsDefault, cancellationToken);
        var isNewScheme = scheme is null;
        if (scheme is null)
        {
            var project = await db.Projects.SingleAsync(x => x.Id == projectId, cancellationToken);
            scheme = new WorkflowScheme { Name = string.IsNullOrWhiteSpace(request.Name) ? $"{project.Name} workflow" : request.Name.Trim(), ProjectId = project.Id, Project = project };
            db.WorkflowSchemes.Add(scheme);
        }
        else scheme.Name = string.IsNullOrWhiteSpace(request.Name) ? scheme.Name : request.Name.Trim();

        try
        {
            if (!isNewScheme)
            {
                var existingTransitions = await db.WorkflowTransitions.IgnoreQueryFilters().Where(x => x.WorkflowSchemeId == scheme.Id).ToListAsync(cancellationToken);
                db.WorkflowTransitions.RemoveRange(existingTransitions);
            }

            foreach (var item in parsed)
                db.WorkflowTransitions.Add(new WorkflowTransition { WorkflowSchemeId = scheme.Id, WorkflowScheme = scheme, FromStatus = item.From, ToStatus = item.To, SortOrder = item.SortOrder });
            if (projectId.HasValue)
            {
                var validStatuses = parsed.SelectMany(x => new[] { x.From, x.To }).ToHashSet();
                var entryStatus = parsed.OrderBy(x => x.SortOrder).First().From;
                var orphanedTasks = await db.Tasks
                    .Where(x => x.ProjectId == projectId && !x.IsDeleted && !validStatuses.Contains(x.Status))
                    .ToListAsync(cancellationToken);
                var actor = User.Identity?.Name ?? "system";
                foreach (var task in orphanedTasks)
                {
                    var previousStatus = task.Status;
                    task.Status = entryStatus;
                    task.UpdatedAt = DateTimeOffset.UtcNow;
                    db.TaskStatusHistory.Add(new TaskStatusHistory
                    {
                        TaskItemId = task.Id,
                        FromStatus = previousStatus,
                        ToStatus = entryStatus,
                        ActorReference = actor,
                        Comment = "Moved to the workflow entry stage because the previous status was removed from the project workflow."
                    });
                    db.AuditEntries.Add(new AuditEntry
                    {
                        EntityName = nameof(TaskItem),
                        EntityId = task.Id.ToString(),
                        Action = $"WorkflowStatusReconciled:{entryStatus}",
                        ActorReference = actor,
                        ChangesJson = JsonSerializer.Serialize(new { fromStatus = previousStatus, toStatus = entryStatus })
                    });
                }
            }
            db.AuditEntries.Add(new AuditEntry { EntityName = nameof(WorkflowScheme), EntityId = scheme.Id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Workflow save failed for project {ProjectId}", projectId);
            return Problem(title: "Workflow could not be saved", detail: exception.GetBaseException().Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        var savedScheme = await db.WorkflowSchemes.AsNoTracking().Include(x => x.Project).Include(x => x.Transitions).SingleAsync(x => x.Id == scheme.Id, cancellationToken);
        return Ok(ToDetails(savedScheme, projectId, false));
    }

    [HttpDelete]
    public async Task<IActionResult> Reset([FromQuery] Guid projectId, CancellationToken cancellationToken)
    {
        var scheme = await db.WorkflowSchemes.Include(x => x.Transitions).SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);
        if (scheme is null) return NoContent();
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(WorkflowScheme), EntityId = scheme.Id.ToString(), Action = "ResetToDefault", ActorReference = User.Identity?.Name ?? "system" });
        db.WorkflowSchemes.Remove(scheme);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static WorkflowDetails ToDetails(WorkflowScheme scheme, Guid? requestedProjectId, bool inherited) => new(scheme.Id, scheme.Name,
        requestedProjectId, inherited ? "Inherited default" : scheme.Project?.Name ?? "System default", scheme.IsDefault, inherited,
        Enum.GetNames<WorkflowStatus>(), scheme.Transitions.Where(x => !x.IsDeleted).OrderBy(x => x.SortOrder).Select(x => new WorkflowTransitionItem(x.Id, x.FromStatus.ToString(), x.ToStatus.ToString(), x.SortOrder)).ToArray());
}
