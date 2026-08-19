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

public sealed record TransitionPermissionRule(string Role, string TaskScope);
public sealed record TransitionPermissionItem(string FromStatus, string ToStatus, IReadOnlyList<TransitionPermissionRule> Rules);
public sealed record SaveTransitionPermissionsRequest(IReadOnlyList<TransitionPermissionItem> Transitions);

[ApiController]
[Route("api/configuration/transition-permissions")]
[Authorize]
public sealed class TransitionPermissionsController(IApplicationDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransitionPermissionItem>>> Get(CancellationToken cancellationToken)
    {
        if (!await IsAdministrator()) return Forbid();
        return Ok(await ReadPolicy(cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<IReadOnlyList<TransitionPermissionItem>>> Save(SaveTransitionPermissionsRequest request, CancellationToken cancellationToken)
    {
        if (!await IsAdministrator()) return Forbid();
        var parsed = Parse(request.Transitions);
        if (parsed.Error is not null) return BadRequest(new { message = parsed.Error });
        var workflowTransitions = await WorkflowTransitionSet(cancellationToken);
        if (!parsed.Permissions.Select(x => (x.From, x.To)).ToHashSet().SetEquals(workflowTransitions))
            return BadRequest(new { message = "Save permissions for every configured workflow transition." });
        await Apply(parsed.Permissions, "TransitionPermissionsUpdated", cancellationToken);
        return Ok(await ReadPolicy(cancellationToken));
    }

    [HttpPost("restore-defaults")]
    public async Task<ActionResult<IReadOnlyList<TransitionPermissionItem>>> RestoreDefaults(CancellationToken cancellationToken)
    {
        if (!await IsAdministrator()) return Forbid();
        var workflowTransitions = await WorkflowTransitionSet(cancellationToken);
        var defaults = UniversalTransitionRolePolicy.Permissions
            .Where(x => workflowTransitions.Contains((x.From, x.To)))
            .Select(x => (x.From, x.To, x.Role, x.TaskScope)).ToList();
        var missing = workflowTransitions.Where(edge => defaults.All(x => x.From != edge.From || x.To != edge.To)).ToArray();
        if (missing.Length > 0)
            return Conflict(new { message = "Some custom workflow transitions have no universal default. Configure their roles manually before saving.", transitions = missing.Select(x => new { fromStatus = x.From, toStatus = x.To }) });
        await Apply(defaults, "TransitionPermissionsRestored", cancellationToken);
        return Ok(await ReadPolicy(cancellationToken));
    }

    private async Task Apply(IReadOnlyList<(WorkflowStatus From, WorkflowStatus To, ProjectRole Role, TaskAccessScope TaskScope)> desired, string action, CancellationToken cancellationToken)
    {
        var existing = await db.TransitionRolePermissions.IgnoreQueryFilters().ToListAsync(cancellationToken);
        var desiredSet = desired.ToDictionary(x => (x.From, x.To, x.Role), x => x.TaskScope);
        foreach (var permission in existing)
        {
            var key = (permission.FromStatus, permission.ToStatus, permission.Role);
            var shouldExist = desiredSet.Remove(key, out var scope);
            permission.IsDeleted = !shouldExist;
            if (shouldExist) permission.TaskScope = scope;
            permission.UpdatedAt = DateTimeOffset.UtcNow;
        }
        foreach (var item in desiredSet)
            db.TransitionRolePermissions.Add(new TransitionRolePermission { FromStatus = item.Key.From, ToStatus = item.Key.To, Role = item.Key.Role, TaskScope = item.Value });
        db.AuditEntries.Add(new AuditEntry
        {
            EntityName = nameof(TransitionRolePermission), EntityId = "GLOBAL", Action = action,
            ActorReference = User.Identity?.Name ?? "system", ChangesJson = JsonSerializer.Serialize(desired.Select(x => new { x.From, x.To, x.Role, x.TaskScope }))
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<TransitionPermissionItem>> ReadPolicy(CancellationToken cancellationToken)
    {
        var workflowTransitions = await WorkflowTransitionSet(cancellationToken);
        var permissions = await db.TransitionRolePermissions.AsNoTracking().ToListAsync(cancellationToken);
        return workflowTransitions.OrderBy(x => x.From).ThenBy(x => x.To)
            .Select(edge => new TransitionPermissionItem(edge.From.ToString(), edge.To.ToString(), permissions
                .Where(x => x.FromStatus == edge.From && x.ToStatus == edge.To).OrderBy(x => x.Role)
                .Select(x => new TransitionPermissionRule(x.Role.ToString(), x.TaskScope.ToString())).ToArray()))
            .ToArray();
    }

    private async Task<HashSet<(WorkflowStatus From, WorkflowStatus To)>> WorkflowTransitionSet(CancellationToken cancellationToken) =>
        (await db.WorkflowTransitions.AsNoTracking().Select(x => new { x.FromStatus, x.ToStatus }).Distinct().ToListAsync(cancellationToken))
        .Select(x => (x.FromStatus, x.ToStatus)).ToHashSet();

    private static (List<(WorkflowStatus From, WorkflowStatus To, ProjectRole Role, TaskAccessScope TaskScope)> Permissions, string? Error) Parse(IReadOnlyList<TransitionPermissionItem>? items)
    {
        if (items is null || items.Count == 0) return ([], "At least one transition is required.");
        var result = new List<(WorkflowStatus, WorkflowStatus, ProjectRole, TaskAccessScope)>();
        foreach (var item in items)
        {
            if (!Enum.TryParse<WorkflowStatus>(item.FromStatus, true, out var from) || !Enum.TryParse<WorkflowStatus>(item.ToStatus, true, out var to)) return ([], "A transition contains an unsupported status.");
            if (item.Rules is null || item.Rules.Count == 0) return ([], $"Assign at least one role to {from} → {to}.");
            foreach (var rule in item.Rules.GroupBy(x => x.Role, StringComparer.OrdinalIgnoreCase).Select(x => x.First()))
            {
                if (!Enum.TryParse<ProjectRole>(rule.Role, true, out var role)) return ([], $"{rule.Role} is not a supported project role.");
                if (!Enum.TryParse<TaskAccessScope>(rule.TaskScope, true, out var scope)) return ([], $"{rule.TaskScope} is not a supported task scope.");
                result.Add((from, to, role, scope));
            }
        }
        if (result.Select(x => (x.Item1, x.Item2)).Distinct().Count() != items.Count) return ([], "Duplicate transitions are not allowed.");
        return (result, null);
    }

    private async Task<bool> IsAdministrator()
    {
        var user = await users.FindByEmailAsync(User.Identity?.Name ?? string.Empty);
        return user is { IsActive: true } && await users.IsInRoleAsync(user, "Administrator");
    }
}
