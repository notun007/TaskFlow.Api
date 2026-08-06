using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;

namespace TaskFlow.Api.Controllers;

public sealed record SoftwareListItem(Guid Id, string Name, string? BusinessOwner, string? TechnicalOwner, string? Criticality, string? Technology, string? CurrentVersion, bool IsProduction, bool IsThirdParty, string? VendorName, int LinkedProjects, int OpenTasks);
public sealed record TeamListItem(Guid Id, string Name, string Department, int AssignedTasks);
public sealed record VendorListItem(Guid Id, string Name, string? SupportEmail, string? SupportPhone, string? ContractReference, string? SlaDetails, int Applications);
public sealed record AuditListItem(Guid Id, string EntityName, string EntityId, string Action, string ActorReference, DateTimeOffset CreatedAt);

[ApiController]
[Route("api/operations")]
[Authorize]
public sealed class OperationsController(IApplicationDbContext db) : ControllerBase
{
    [HttpGet("software")]
    public async Task<ActionResult<IReadOnlyList<SoftwareListItem>>> Software(CancellationToken cancellationToken) =>
        Ok(await db.SoftwareApplications.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name)
            .Select(item => new SoftwareListItem(
                item.Id, item.Name, item.BusinessOwner, item.TechnicalOwner, item.Criticality,
                item.Technology, item.CurrentVersion, item.IsProduction, item.IsThirdParty,
                item.Vendor != null ? item.Vendor.Name : null,
                db.Projects.Count(project => !project.IsDeleted && project.SoftwareApplicationId == item.Id),
                db.Tasks.Count(task => !task.IsDeleted && task.SoftwareApplicationId == item.Id && task.Status != TaskFlow.Domain.Enums.TaskStatus.Closed && task.Status != TaskFlow.Domain.Enums.TaskStatus.Cancelled)))
            .ToListAsync(cancellationToken));

    [HttpGet("teams")]
    public async Task<ActionResult<IReadOnlyList<TeamListItem>>> Teams(CancellationToken cancellationToken) =>
        Ok(await db.Teams.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name)
            .Select(item => new TeamListItem(item.Id, item.Name, item.Department != null ? item.Department.Name : "Unassigned", db.TaskAssignments.Count(assignment => !assignment.IsDeleted && assignment.PartyReference == item.Name)))
            .ToListAsync(cancellationToken));

    [HttpGet("vendors")]
    public async Task<ActionResult<IReadOnlyList<VendorListItem>>> Vendors(CancellationToken cancellationToken) =>
        Ok(await db.Vendors.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name)
            .Select(item => new VendorListItem(item.Id, item.Name, item.SupportEmail, item.SupportPhone, item.ContractReference, item.SlaDetails, db.SoftwareApplications.Count(application => !application.IsDeleted && application.VendorId == item.Id)))
            .ToListAsync(cancellationToken));

    [HttpGet("audit")]
    public async Task<ActionResult<IReadOnlyList<AuditListItem>>> Audit([FromQuery] int take = 100, CancellationToken cancellationToken = default) =>
        Ok(await db.AuditEntries.AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .Take(Math.Clamp(take, 10, 250))
            .Select(item => new AuditListItem(item.Id, item.EntityName, item.EntityId, item.Action, item.ActorReference, item.CreatedAt))
            .ToListAsync(cancellationToken));
}
