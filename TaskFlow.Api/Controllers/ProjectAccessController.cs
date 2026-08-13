using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Api.Controllers;

public sealed record ProjectAccessUser(Guid Id, string Email, string DisplayName, bool IsActive, IReadOnlyList<string> Roles);
public sealed record AddProjectRoleRequest(Guid UserId, string Role);

[ApiController]
[Route("api/projects/{projectId:guid}/access")]
[Authorize]
public sealed class ProjectAccessController(IApplicationDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectAccessUser>>> Get(Guid projectId, CancellationToken cancellationToken)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken)) return NotFound();
        var assignments = await db.ProjectRoleAssignments.AsNoTracking().Where(x => x.ProjectId == projectId && !x.IsDeleted).ToListAsync(cancellationToken);
        var accounts = await users.Users.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.DisplayName).ThenBy(x => x.Email).ToListAsync(cancellationToken);
        return Ok(accounts.Select(account => new ProjectAccessUser(account.Id, account.Email ?? string.Empty,
            account.DisplayName ?? account.Email ?? "TaskFlow user", account.IsActive,
            assignments.Where(x => x.UserId == account.Id).OrderBy(x => x.Role).Select(x => x.Role.ToString()).ToArray())).ToArray());
    }

    [HttpPost("roles")]
    public async Task<IActionResult> AddRole(Guid projectId, AddProjectRoleRequest request, CancellationToken cancellationToken)
    {
        if (!await IsAdministrator()) return Forbid();
        if (!await db.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken)) return NotFound();
        if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role)) return BadRequest(new { message = "Select a supported project role." });
        if (!await users.Users.AnyAsync(x => x.Id == request.UserId && x.IsActive, cancellationToken)) return BadRequest(new { message = "Select an active user." });
        if (await db.ProjectRoleAssignments.AnyAsync(x => x.ProjectId == projectId && x.UserId == request.UserId && x.Role == role && !x.IsDeleted, cancellationToken)) return NoContent();
        var assignment = new ProjectRoleAssignment { ProjectId = projectId, UserId = request.UserId, Role = role };
        db.ProjectRoleAssignments.Add(assignment);
        Audit(assignment, "ProjectRoleAssigned");
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRole(Guid projectId, Guid userId, string roleName, CancellationToken cancellationToken)
    {
        if (!await IsAdministrator()) return Forbid();
        if (!Enum.TryParse<ProjectRole>(roleName, true, out var role)) return BadRequest(new { message = "Select a supported project role." });
        var assignment = await db.ProjectRoleAssignments.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId && x.Role == role && !x.IsDeleted, cancellationToken);
        if (assignment is null) return NotFound();
        assignment.IsDeleted = true;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        Audit(assignment, "ProjectRoleRemoved");
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private void Audit(ProjectRoleAssignment assignment, string action) => db.AuditEntries.Add(new AuditEntry
    {
        EntityName = nameof(ProjectRoleAssignment), EntityId = assignment.Id.ToString(), Action = action,
        ActorReference = User.Identity?.Name ?? "system",
        ChangesJson = JsonSerializer.Serialize(new { assignment.ProjectId, assignment.UserId, assignment.Role })
    });

    private async Task<bool> IsAdministrator()
    {
        var currentUser = await users.FindByEmailAsync(User.Identity?.Name ?? string.Empty);
        return currentUser is { IsActive: true } && await users.IsInRoleAsync(currentUser, "Administrator");
    }
}
