using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;

namespace TaskFlow.Api.Controllers;

public sealed record ReferenceItem(Guid Id, string Name);

[ApiController]
[Route("api/reference-data")]
[Authorize]
public sealed class ReferenceDataController(IApplicationDbContext db) : ControllerBase
{
    [HttpGet("projects")]
    public async Task<ActionResult<IReadOnlyList<ReferenceItem>>> Projects(CancellationToken cancellationToken) =>
        Ok(await db.Projects.AsNoTracking()
            .Where(project => !project.IsDeleted && project.Status == "Active")
            .OrderBy(project => project.Name)
            .Select(project => new ReferenceItem(project.Id, project.Name))
            .ToListAsync(cancellationToken));

    [HttpGet("software-applications")]
    public async Task<ActionResult<IReadOnlyList<ReferenceItem>>> SoftwareApplications(CancellationToken cancellationToken) =>
        Ok(await db.SoftwareApplications.AsNoTracking()
            .Where(application => !application.IsDeleted)
            .OrderBy(application => application.Name)
            .Select(application => new ReferenceItem(application.Id, application.Name))
            .ToListAsync(cancellationToken));
}
