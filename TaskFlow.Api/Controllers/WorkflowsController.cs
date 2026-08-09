using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

public sealed record WorkflowTransitionItem(Guid Id, string FromStatus, string ToStatus, int SortOrder);
public sealed record WorkflowDetails(Guid Id, string Name, Guid? WorkItemTypeId, string WorkItemType, bool IsDefault, bool IsInherited,
    IReadOnlyList<string> Statuses, IReadOnlyList<WorkflowTransitionItem> Transitions);
public sealed record SaveWorkflowTransitionRequest(string FromStatus, string ToStatus, int SortOrder);
public sealed record SaveWorkflowRequest(string Name, IReadOnlyList<SaveWorkflowTransitionRequest> Transitions);

[ApiController]
[Route("api/configuration/workflows")]
[Authorize]
public sealed class WorkflowsController(IApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<WorkflowDetails>> Get([FromQuery] Guid? workItemTypeId, CancellationToken cancellationToken)
    {
        if (workItemTypeId.HasValue && !await db.WorkItemTypes.AnyAsync(x => x.Id == workItemTypeId && x.IsActive, cancellationToken)) return NotFound();
        var scheme = workItemTypeId.HasValue
            ? await db.WorkflowSchemes.AsNoTracking().Include(x => x.WorkItemType).Include(x => x.Transitions).SingleOrDefaultAsync(x => x.WorkItemTypeId == workItemTypeId, cancellationToken)
            : await db.WorkflowSchemes.AsNoTracking().Include(x => x.Transitions).SingleAsync(x => x.IsDefault, cancellationToken);
        var inherited = scheme is null;
        scheme ??= await db.WorkflowSchemes.AsNoTracking().Include(x => x.Transitions).SingleAsync(x => x.IsDefault, cancellationToken);
        return Ok(ToDetails(scheme, workItemTypeId, inherited));
    }

    [HttpPut]
    public async Task<ActionResult<WorkflowDetails>> Save([FromQuery] Guid? workItemTypeId, SaveWorkflowRequest request, CancellationToken cancellationToken)
    {
        if (workItemTypeId.HasValue && !await db.WorkItemTypes.AnyAsync(x => x.Id == workItemTypeId && x.IsActive, cancellationToken)) return NotFound();
        var parsed = new List<(WorkflowStatus From, WorkflowStatus To, int SortOrder)>();
        foreach (var transition in request.Transitions ?? [])
        {
            if (!Enum.TryParse<WorkflowStatus>(transition.FromStatus, true, out var from) || !Enum.TryParse<WorkflowStatus>(transition.ToStatus, true, out var to)) return BadRequest(new { message = "A transition contains an unsupported status." });
            if (from == to) return BadRequest(new { message = "A workflow cannot transition a status to itself." });
            parsed.Add((from, to, transition.SortOrder));
        }
        if (parsed.Count == 0) return BadRequest(new { message = "Add at least one workflow transition." });
        if (parsed.GroupBy(x => new { x.From, x.To }).Any(x => x.Count() > 1)) return BadRequest(new { message = "Duplicate workflow transitions are not allowed." });

        var scheme = workItemTypeId.HasValue
            ? await db.WorkflowSchemes.Include(x => x.WorkItemType).Include(x => x.Transitions).SingleOrDefaultAsync(x => x.WorkItemTypeId == workItemTypeId, cancellationToken)
            : await db.WorkflowSchemes.Include(x => x.Transitions).SingleAsync(x => x.IsDefault, cancellationToken);
        if (scheme is null)
        {
            var type = await db.WorkItemTypes.SingleAsync(x => x.Id == workItemTypeId, cancellationToken);
            scheme = new WorkflowScheme { Name = string.IsNullOrWhiteSpace(request.Name) ? $"{type.Name} workflow" : request.Name.Trim(), WorkItemTypeId = type.Id, WorkItemType = type };
            db.WorkflowSchemes.Add(scheme);
        }
        else scheme.Name = string.IsNullOrWhiteSpace(request.Name) ? scheme.Name : request.Name.Trim();

        foreach (var existing in scheme.Transitions) existing.IsDeleted = true;
        foreach (var item in parsed)
        {
            var transition = scheme.Transitions.FirstOrDefault(x => x.FromStatus == item.From && x.ToStatus == item.To);
            if (transition is null) scheme.Transitions.Add(new WorkflowTransition { FromStatus = item.From, ToStatus = item.To, SortOrder = item.SortOrder });
            else { transition.SortOrder = item.SortOrder; transition.IsDeleted = false; }
        }
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(WorkflowScheme), EntityId = scheme.Id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDetails(scheme, workItemTypeId, false));
    }

    [HttpDelete]
    public async Task<IActionResult> Reset([FromQuery] Guid workItemTypeId, CancellationToken cancellationToken)
    {
        var scheme = await db.WorkflowSchemes.Include(x => x.Transitions).SingleOrDefaultAsync(x => x.WorkItemTypeId == workItemTypeId, cancellationToken);
        if (scheme is null) return NoContent();
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(WorkflowScheme), EntityId = scheme.Id.ToString(), Action = "ResetToDefault", ActorReference = User.Identity?.Name ?? "system" });
        db.WorkflowSchemes.Remove(scheme);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static WorkflowDetails ToDetails(WorkflowScheme scheme, Guid? requestedTypeId, bool inherited) => new(scheme.Id, scheme.Name,
        requestedTypeId, inherited ? "Inherited default" : scheme.WorkItemType?.Name ?? "All work item types", scheme.IsDefault, inherited,
        Enum.GetNames<WorkflowStatus>(), scheme.Transitions.Where(x => !x.IsDeleted).OrderBy(x => x.SortOrder).Select(x => new WorkflowTransitionItem(x.Id, x.FromStatus.ToString(), x.ToStatus.ToString(), x.SortOrder)).ToArray());
}
