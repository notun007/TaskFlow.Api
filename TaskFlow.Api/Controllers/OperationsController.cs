using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Api.Controllers;

public sealed record SoftwareListItem(Guid Id, string Name, string? BusinessOwner, string? TechnicalOwner, string? SupportTeam, string? Criticality, string? Technology, string? CurrentVersion, bool IsProduction, bool IsThirdParty, Guid? VendorId, string? VendorName, int LinkedProjects, int OpenTasks);
public sealed record SaveSoftwareRequest(string Name, string? BusinessOwner, string? TechnicalOwner, string? SupportTeam, string? Criticality, string? Technology, string? CurrentVersion, bool IsProduction, bool IsThirdParty, Guid? VendorId);
public sealed record TeamListItem(Guid Id, string Name, Guid DepartmentId, string Department, int AssignedTasks);
public sealed record SaveTeamRequest(string Name, Guid DepartmentId);
public sealed record DepartmentListItem(Guid Id, string Name, string? Description, int Teams);
public sealed record SaveDepartmentRequest(string Name, string? Description);
public sealed record VendorListItem(Guid Id, string Name, string? SupportEmail, string? SupportPhone, string? ContractReference, string? SlaDetails, int Applications);
public sealed record SaveVendorRequest(string Name, string? SupportEmail, string? SupportPhone, string? ContractReference, string? SlaDetails);
public sealed record AuditListItem(Guid Id, string EntityName, string EntityId, string Action, string ActorReference, DateTimeOffset CreatedAt);

[ApiController]
[Route("api/operations")]
[Authorize]
public sealed class OperationsController(IApplicationDbContext db) : ControllerBase
{
    [HttpGet("departments")]
    public async Task<ActionResult<IReadOnlyList<DepartmentListItem>>> Departments(CancellationToken cancellationToken) =>
        Ok(await db.Departments.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name)
            .Select(item => new DepartmentListItem(item.Id, item.Name, item.Description, db.Teams.Count(team => !team.IsDeleted && team.DepartmentId == item.Id)))
            .ToListAsync(cancellationToken));

    [HttpPost("departments")]
    public async Task<ActionResult<DepartmentListItem>> CreateDepartment(SaveDepartmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Department name is required." });
        var department = new Department { Name = request.Name.Trim(), Description = request.Description?.Trim() };
        db.Departments.Add(department);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Department), EntityId = department.Id.ToString(), Action = "Created", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Departments), await FindDepartment(department.Id, cancellationToken));
    }

    [HttpPut("departments/{id:guid}")]
    public async Task<ActionResult<DepartmentListItem>> UpdateDepartment(Guid id, SaveDepartmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Department name is required." });
        var department = await db.Departments.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (department is null) return NotFound();
        department.Name = request.Name.Trim();
        department.Description = request.Description?.Trim();
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Department), EntityId = id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(await FindDepartment(id, cancellationToken));
    }

    private async Task<DepartmentListItem> FindDepartment(Guid id, CancellationToken cancellationToken) =>
        await db.Departments.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new DepartmentListItem(item.Id, item.Name, item.Description, db.Teams.Count(team => !team.IsDeleted && team.DepartmentId == item.Id)))
            .SingleAsync(cancellationToken);

    [HttpGet("software")]
    public async Task<ActionResult<IReadOnlyList<SoftwareListItem>>> Software(CancellationToken cancellationToken) =>
        Ok(await db.SoftwareApplications.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name)
            .Select(item => new SoftwareListItem(
                item.Id, item.Name, item.BusinessOwner, item.TechnicalOwner, item.SupportTeam, item.Criticality,
                item.Technology, item.CurrentVersion, item.IsProduction, item.IsThirdParty, item.VendorId,
                item.Vendor != null ? item.Vendor.Name : null,
                db.Projects.Count(project => !project.IsDeleted && project.SoftwareApplicationId == item.Id),
                db.Tasks.Count(task => !task.IsDeleted && task.SoftwareApplicationId == item.Id && task.Status != TaskFlow.Domain.Enums.TaskStatus.Closed && task.Status != TaskFlow.Domain.Enums.TaskStatus.Cancelled)))
            .ToListAsync(cancellationToken));

    [HttpPost("software")]
    public async Task<ActionResult<SoftwareListItem>> CreateSoftware(SaveSoftwareRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Software name is required." });
        var software = new SoftwareApplication { Name = request.Name.Trim() };
        ApplySoftware(software, request);
        db.SoftwareApplications.Add(software);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(SoftwareApplication), EntityId = software.Id.ToString(), Action = "Created", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Software), await FindSoftware(software.Id, cancellationToken));
    }

    [HttpPut("software/{id:guid}")]
    public async Task<ActionResult<SoftwareListItem>> UpdateSoftware(Guid id, SaveSoftwareRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Software name is required." });
        var software = await db.SoftwareApplications.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (software is null) return NotFound();
        ApplySoftware(software, request);
        software.UpdatedAt = DateTimeOffset.UtcNow;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(SoftwareApplication), EntityId = id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(await FindSoftware(id, cancellationToken));
    }

    private static void ApplySoftware(SoftwareApplication software, SaveSoftwareRequest request)
    {
        software.Name = request.Name.Trim();
        software.BusinessOwner = request.BusinessOwner?.Trim();
        software.TechnicalOwner = request.TechnicalOwner?.Trim();
        software.SupportTeam = request.SupportTeam?.Trim();
        software.Criticality = request.Criticality?.Trim();
        software.Technology = request.Technology?.Trim();
        software.CurrentVersion = request.CurrentVersion?.Trim();
        software.IsProduction = request.IsProduction;
        software.IsThirdParty = request.IsThirdParty;
        software.VendorId = request.IsThirdParty ? request.VendorId : null;
    }

    private async Task<SoftwareListItem> FindSoftware(Guid id, CancellationToken cancellationToken) =>
        await db.SoftwareApplications.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new SoftwareListItem(item.Id, item.Name, item.BusinessOwner, item.TechnicalOwner, item.SupportTeam, item.Criticality, item.Technology, item.CurrentVersion, item.IsProduction, item.IsThirdParty, item.VendorId, item.Vendor != null ? item.Vendor.Name : null, db.Projects.Count(project => !project.IsDeleted && project.SoftwareApplicationId == item.Id), db.Tasks.Count(task => !task.IsDeleted && task.SoftwareApplicationId == item.Id && task.Status != TaskFlow.Domain.Enums.TaskStatus.Closed && task.Status != TaskFlow.Domain.Enums.TaskStatus.Cancelled)))
            .SingleAsync(cancellationToken);

    [HttpGet("teams")]
    public async Task<ActionResult<IReadOnlyList<TeamListItem>>> Teams(CancellationToken cancellationToken) =>
        Ok(await db.Teams.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name)
            .Select(item => new TeamListItem(item.Id, item.Name, item.DepartmentId, item.Department != null ? item.Department.Name : "Unassigned", db.TaskAssignments.Count(assignment => !assignment.IsDeleted && assignment.PartyReference == item.Name)))
            .ToListAsync(cancellationToken));

    [HttpPost("teams")]
    public async Task<ActionResult<TeamListItem>> CreateTeam(SaveTeamRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateTeam(request, cancellationToken);
        if (validation is not null) return validation;
        var team = new Team { Name = request.Name.Trim(), DepartmentId = request.DepartmentId };
        db.Teams.Add(team);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Team), EntityId = team.Id.ToString(), Action = "Created", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Teams), await FindTeam(team.Id, cancellationToken));
    }

    [HttpPut("teams/{id:guid}")]
    public async Task<ActionResult<TeamListItem>> UpdateTeam(Guid id, SaveTeamRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateTeam(request, cancellationToken);
        if (validation is not null) return validation;
        var team = await db.Teams.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (team is null) return NotFound();
        team.Name = request.Name.Trim();
        team.DepartmentId = request.DepartmentId;
        team.UpdatedAt = DateTimeOffset.UtcNow;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Team), EntityId = id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(await FindTeam(id, cancellationToken));
    }

    private async Task<ActionResult?> ValidateTeam(SaveTeamRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Team name is required." });
        if (!await db.Departments.AnyAsync(item => item.Id == request.DepartmentId && !item.IsDeleted, cancellationToken)) return BadRequest(new { message = "Select a valid department." });
        return null;
    }

    private async Task<TeamListItem> FindTeam(Guid id, CancellationToken cancellationToken) =>
        await db.Teams.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new TeamListItem(item.Id, item.Name, item.DepartmentId, item.Department != null ? item.Department.Name : "Unassigned", db.TaskAssignments.Count(assignment => !assignment.IsDeleted && assignment.PartyReference == item.Name)))
            .SingleAsync(cancellationToken);

    [HttpGet("vendors")]
    public async Task<ActionResult<IReadOnlyList<VendorListItem>>> Vendors(CancellationToken cancellationToken) =>
        Ok(await db.Vendors.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name)
            .Select(item => new VendorListItem(item.Id, item.Name, item.SupportEmail, item.SupportPhone, item.ContractReference, item.SlaDetails, db.SoftwareApplications.Count(application => !application.IsDeleted && application.VendorId == item.Id)))
            .ToListAsync(cancellationToken));

    [HttpPost("vendors")]
    public async Task<ActionResult<VendorListItem>> CreateVendor(SaveVendorRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Vendor name is required." });
        var vendor = new Vendor { Name = request.Name.Trim() };
        ApplyVendor(vendor, request);
        db.Vendors.Add(vendor);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Vendor), EntityId = vendor.Id.ToString(), Action = "Created", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Vendors), await FindVendor(vendor.Id, cancellationToken));
    }

    [HttpPut("vendors/{id:guid}")]
    public async Task<ActionResult<VendorListItem>> UpdateVendor(Guid id, SaveVendorRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Vendor name is required." });
        var vendor = await db.Vendors.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (vendor is null) return NotFound();
        ApplyVendor(vendor, request);
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Vendor), EntityId = id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(await FindVendor(id, cancellationToken));
    }

    private static void ApplyVendor(Vendor vendor, SaveVendorRequest request)
    {
        vendor.Name = request.Name.Trim();
        vendor.SupportEmail = request.SupportEmail?.Trim();
        vendor.SupportPhone = request.SupportPhone?.Trim();
        vendor.ContractReference = request.ContractReference?.Trim();
        vendor.SlaDetails = request.SlaDetails?.Trim();
    }

    private async Task<VendorListItem> FindVendor(Guid id, CancellationToken cancellationToken) =>
        await db.Vendors.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new VendorListItem(item.Id, item.Name, item.SupportEmail, item.SupportPhone, item.ContractReference, item.SlaDetails, db.SoftwareApplications.Count(application => !application.IsDeleted && application.VendorId == item.Id)))
            .SingleAsync(cancellationToken);

    [HttpGet("audit")]
    public async Task<ActionResult<IReadOnlyList<AuditListItem>>> Audit([FromQuery] int take = 100, CancellationToken cancellationToken = default) =>
        Ok(await db.AuditEntries.AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .Take(Math.Clamp(take, 10, 250))
            .Select(item => new AuditListItem(item.Id, item.EntityName, item.EntityId, item.Action, item.ActorReference, item.CreatedAt))
            .ToListAsync(cancellationToken));
}
