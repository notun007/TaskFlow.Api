using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Api.Controllers;

public sealed record CreateProjectRequest(string Name, string? ProjectKey, string? Objectives);
public sealed record UpdateProjectRequest(string Name, string? ProjectKey, string? Objectives, string Status, DateOnly? StartDate, DateOnly? TargetDate, string? ProjectManager, string? Sponsor, Guid? SoftwareApplicationId);
public sealed record ProjectListItem(Guid Id, string Name, string? ProjectKey, string Status, DateOnly? TargetDate, string? ProjectManager, int TaskCount);
public sealed record ProjectDetails(Guid Id, string Name, string? ProjectKey, string? Objectives, string Status, DateOnly? StartDate, DateOnly? TargetDate, string? ProjectManager, string? Sponsor, Guid? SoftwareApplicationId, string? SoftwareApplicationName, int TaskCount, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController(IApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectListItem>>> List(CancellationToken cancellationToken) =>
        Ok(await db.Projects.AsNoTracking()
            .Where(project => !project.IsDeleted)
            .OrderBy(project => project.Name)
            .Select(project => new ProjectListItem(project.Id, project.Name, project.ProjectKey, project.Status, project.TargetDate, project.ProjectManager, project.Tasks.Count(task => !task.IsDeleted)))
            .ToListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDetails>> Get(Guid id, CancellationToken cancellationToken)
    {
        var project = await db.Projects.AsNoTracking()
            .Where(item => item.Id == id && !item.IsDeleted)
            .Select(item => new ProjectDetails(item.Id, item.Name, item.ProjectKey, item.Objectives, item.Status, item.StartDate, item.TargetDate, item.ProjectManager, item.Sponsor, item.SoftwareApplicationId, item.SoftwareApplication != null ? item.SoftwareApplication.Name : null, item.Tasks.Count(task => !task.IsDeleted), item.CreatedAt, item.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ReferenceItem>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Project name is required." });
        if (await db.Projects.AnyAsync(project => !project.IsDeleted && project.Name == name, cancellationToken))
            return Conflict(new { message = "A project with this name already exists." });

        var project = new Project
        {
            Name = name,
            ProjectKey = string.IsNullOrWhiteSpace(request.ProjectKey) ? null : request.ProjectKey.Trim(),
            Objectives = string.IsNullOrWhiteSpace(request.Objectives) ? null : request.Objectives.Trim()
        };
        db.Projects.Add(project);
        db.AuditEntries.Add(new AuditEntry
        {
            EntityName = nameof(Project),
            EntityId = project.Id.ToString(),
            Action = "Created",
            ActorReference = User.Identity?.Name ?? "system"
        });
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/projects/{project.Id}", new ReferenceItem(project.Id, project.Name));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDetails>> Update(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await db.Projects.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (project is null) return NotFound();
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Project name is required." });
        if (await db.Projects.AnyAsync(item => item.Id != id && !item.IsDeleted && item.Name == name, cancellationToken)) return Conflict(new { message = "A project with this name already exists." });
        project.Name = name;
        project.ProjectKey = string.IsNullOrWhiteSpace(request.ProjectKey) ? null : request.ProjectKey.Trim();
        project.Objectives = string.IsNullOrWhiteSpace(request.Objectives) ? null : request.Objectives.Trim();
        project.Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();
        project.StartDate = request.StartDate;
        project.TargetDate = request.TargetDate;
        project.ProjectManager = string.IsNullOrWhiteSpace(request.ProjectManager) ? null : request.ProjectManager.Trim();
        project.Sponsor = string.IsNullOrWhiteSpace(request.Sponsor) ? null : request.Sponsor.Trim();
        project.SoftwareApplicationId = request.SoftwareApplicationId;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Project), EntityId = id.ToString(), Action = "Updated", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return await Get(id, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var project = await db.Projects.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (project is null) return NotFound();
        project.IsDeleted = true;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(Project), EntityId = id.ToString(), Action = "Archived", ActorReference = User.Identity?.Name ?? "system" });
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
