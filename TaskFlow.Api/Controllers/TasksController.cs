using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskFlow.Application.Tasks;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Api.Controllers;

public sealed record AddTaskCommentRequest(string Body);
public sealed record ChangeTaskStatusRequest(WorkflowStatus Status, string? Comment, bool RequireActiveSprint = false);
public sealed record CreateSubtaskRequest(string Title, string? Description, string Type, Priority Priority, Severity? Severity, DateTimeOffset? DueDate, Guid? SprintId, IReadOnlyList<SaveTaskCustomFieldValueRequest>? CustomFields, Guid? OwnerUserId = null, int? EstimatedEffortMinutes = null);

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(ITaskService service, IApplicationDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    private const long MaxAttachmentSize = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedAttachmentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/png", "image/jpeg", "image/gif", "text/plain", "text/csv",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskListItem>>> List(
        [FromQuery] string? search,
        [FromQuery] WorkflowStatus? status,
        [FromQuery] Priority? priority,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? epicId,
        [FromQuery] string? epicAssignment,
        [FromQuery] Guid? featureId,
        [FromQuery] string? featureAssignment,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await service.ListAsync(search, status, priority, projectId, epicId, epicAssignment, featureId, featureAssignment, sortBy, sortDirection, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDetails>> Get(Guid id, CancellationToken cancellationToken) => (await service.GetAsync(id, cancellationToken, CurrentUserId(), User.Identity?.Name)) is { } task ? Ok(task) : NotFound();

    [HttpPost]
    public async Task<ActionResult> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var identity = await CurrentIdentity(cancellationToken);
        if (identity is null) return Unauthorized();
        var ownerName = await ResolveOwnerName(request.ProjectId, request.OwnerUserId, cancellationToken);
        if (request.OwnerUserId.HasValue && ownerName is null) return BadRequest(new { message = "Owner must be an active member of the selected project." });
        var task = await service.CreateAsync(request, User.Identity?.Name ?? "system", identity.Value.Id, identity.Value.Name, ownerName, cancellationToken);
        if (task is null) return BadRequest(new { message = "A title and active work item type are required." });
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPost("{id:guid}/subtasks")]
    public async Task<ActionResult<TaskDetails>> CreateSubtask(Guid id, CreateSubtaskRequest request, CancellationToken cancellationToken)
    {
        var parent = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (parent is null) return NotFound();
        if (parent.ParentTaskId.HasValue) return Conflict(new { message = "A subtask cannot have child tasks. Only one subtask level is allowed." });
        if (request.SprintId.HasValue && !await db.Sprints.AnyAsync(x => x.Id == request.SprintId && x.ProjectId == parent.ProjectId && !x.IsDeleted && x.Status != SprintStatus.Completed, cancellationToken))
            return BadRequest(new { message = "Select a planned or active sprint from the parent task's project." });
        var identity = await CurrentIdentity(cancellationToken);
        if (identity is null) return Unauthorized();
        var ownerName = await ResolveOwnerName(parent.ProjectId, request.OwnerUserId, cancellationToken);
        if (request.OwnerUserId.HasValue && ownerName is null) return BadRequest(new { message = "Owner must be an active member of the selected project." });
        var created = await service.CreateAsync(new CreateTaskRequest(request.Title, request.Description, request.Type, request.Priority, request.Severity, parent.ProjectId, parent.SoftwareApplicationId, request.DueDate, parent.EpicId, parent.FeatureId, request.CustomFields, request.OwnerUserId, request.EstimatedEffortMinutes), User.Identity?.Name ?? "system", identity.Value.Id, identity.Value.Name, ownerName, cancellationToken);
        if (created is null) return BadRequest(new { message = "A title, valid work item type, and valid required fields are required." });
        created.ParentTaskId = parent.Id;
        created.SprintId = request.SprintId;
        db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = created.Id.ToString(), Action = "SubtaskCreated", ActorReference = User.Identity?.Name ?? "system", ChangesJson = System.Text.Json.JsonSerializer.Serialize(new { parentTaskId = parent.Id, sprintId = request.SprintId }) });
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, await service.GetAsync(created.Id, cancellationToken, CurrentUserId(), User.Identity?.Name));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDetails>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var identity = await CurrentIdentity(cancellationToken);
        if (identity is null) return Unauthorized();
        var existing = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (existing is null) return NotFound();
        if (request.ProjectId != existing.ProjectId) return BadRequest(new { message = "A task cannot be moved to another project." });
        if (!await CanEditTask(existing, identity.Value.Id, cancellationToken))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can edit only tasks you reported or own. Project Leads, Product Owners, and Project Admins may edit all project tasks." });
        if (request.OwnerUserId != existing.OwnerUserId && !await CanChangeOwner(existing.ProjectId, identity.Value.Id, cancellationToken))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only a Requester, Team Lead, Product Owner, or Project Admin can change the task owner." });
        var ownerName = await ResolveOwnerName(request.ProjectId, request.OwnerUserId, cancellationToken);
        if (request.OwnerUserId.HasValue && ownerName is null) return BadRequest(new { message = "Owner must be an active member of the selected project." });
        return (await service.UpdateAsync(id, request, User.Identity?.Name ?? "system", ownerName, cancellationToken)) is { } task ? Ok(task) : BadRequest(new { message = "Task data is invalid." });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskDetails>> ChangeStatus(Guid id, ChangeTaskStatusRequest request, CancellationToken cancellationToken)
    {
        if (request.RequireActiveSprint)
        {
            var sprintId = await db.Tasks.Where(task => task.Id == id && !task.IsDeleted)
                .Select(task => task.SprintId).SingleOrDefaultAsync(cancellationToken);
            var activeSprint = sprintId.HasValue && await db.Sprints.AnyAsync(sprint =>
                sprint.Id == sprintId.Value && !sprint.IsDeleted && sprint.Status == SprintStatus.Active,
                cancellationToken);
            if (!activeSprint)
                return Conflict(new { message = "Board movement is allowed only for tasks in an active sprint.", allowedTransitions = Array.Empty<WorkflowStatus>() });
        }
        var result = await service.ChangeStatusAsync(id, request.Status, request.Comment, User.Identity?.Name ?? "system", cancellationToken, CurrentUserId());
        return result.Outcome switch
        {
            TaskStatusChangeOutcome.Changed => Ok(result.Task),
            TaskStatusChangeOutcome.NotFound => NotFound(),
            TaskStatusChangeOutcome.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = "Your project roles do not permit this transition.", requestedStatus = request.Status, requiredRoles = result.RequiredRoles, allowedTransitions = result.AllowedTransitions }),
            TaskStatusChangeOutcome.IncompleteSubtasks => Conflict(new { message = "Complete or cancel every subtask before resolving or closing the parent task.", requestedStatus = request.Status, allowedTransitions = result.AllowedTransitions }),
            TaskStatusChangeOutcome.ReasonRequired => BadRequest(new { message = "Enter a reason before returning or reopening this task.", requestedStatus = request.Status, allowedTransitions = result.AllowedTransitions }),
            _ => Conflict(new { message = "This workflow transition is not allowed.", requestedStatus = request.Status, allowedTransitions = result.AllowedTransitions })
        };
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<TaskCommentItem>> AddComment(Guid id, AddTaskCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await service.AddCommentAsync(id, request.Body, User.Identity?.Name ?? "system", cancellationToken);
        return comment is null ? BadRequest(new { message = "A valid task and comment body are required." }) : Ok(comment);
    }

    [HttpPost("{id:guid}/assignments")]
    public async Task<ActionResult<TaskAssignmentItem>> AddAssignment(Guid id, AddTaskAssignmentRequest request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (task is null) return NotFound();
        var currentUserId = CurrentUserId();
        if (!currentUserId.HasValue || !await CanManageAssignments(task.ProjectId, currentUserId.Value, cancellationToken))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only a Team Lead, Product Owner, or Project Admin can manage task responsibilities." });
        if (request.Responsibility is ResponsibilityType.Assignee or ResponsibilityType.Tester or ResponsibilityType.UatOwner or ResponsibilityType.Approver)
        {
            var reference = request.PartyReference.Trim();
            var assignedUserId = request.AssignedUserId;
            if (!assignedUserId.HasValue && Guid.TryParse(reference, out var parsedUserId)) assignedUserId = parsedUserId;
            var assignedUser = assignedUserId.HasValue
                ? await userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.IsActive && x.Id == assignedUserId.Value, cancellationToken)
                : await userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.IsActive && x.Email == reference, cancellationToken);
            if (assignedUser is null || !await db.ProjectRoleAssignments.AnyAsync(x => x.ProjectId == task.ProjectId && x.UserId == assignedUser.Id && !x.IsDeleted, cancellationToken))
                return BadRequest(new { message = "Select an active user who has a role in this project." });
            request = request with { AssignedUserId = assignedUser.Id, PartyReference = assignedUser.Email!, DisplayName = string.IsNullOrWhiteSpace(assignedUser.DisplayName) ? assignedUser.Email : assignedUser.DisplayName };
        }
        var assignment = await service.AddAssignmentAsync(id, request, User.Identity?.Name ?? "system", cancellationToken);
        return assignment is null ? BadRequest(new { message = "A valid task and party reference are required." }) : Ok(assignment);
    }

    [HttpDelete("{id:guid}/assignments/{assignmentId:guid}")]
    public async Task<IActionResult> RemoveAssignment(Guid id, Guid assignmentId, CancellationToken cancellationToken)
    {
        var projectId = await db.Tasks.AsNoTracking().Where(x => x.Id == id && !x.IsDeleted).Select(x => (Guid?)x.ProjectId).SingleOrDefaultAsync(cancellationToken);
        if (!projectId.HasValue) return NotFound();
        var currentUserId = CurrentUserId();
        if (!currentUserId.HasValue || !await CanManageAssignments(projectId.Value, currentUserId.Value, cancellationToken))
            return StatusCode(StatusCodes.Status403Forbidden);
        return await service.RemoveAssignmentAsync(id, assignmentId, User.Identity?.Name ?? "system", cancellationToken) ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/links")]
    public async Task<ActionResult<TaskLinkItem>> AddLink(Guid id, AddTaskLinkRequest request, CancellationToken cancellationToken)
    {
        var link = await service.AddLinkAsync(id, request, User.Identity?.Name ?? "system", cancellationToken);
        return link is null ? BadRequest(new { message = "Use a valid different task reference. A child can only have one parent." }) : Ok(link);
    }

    [HttpDelete("{id:guid}/links/{linkId:guid}")]
    public async Task<IActionResult> RemoveLink(Guid id, Guid linkId, CancellationToken cancellationToken) =>
        await service.RemoveLinkAsync(id, linkId, User.Identity?.Name ?? "system", cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/attachments")]
    [RequestSizeLimit(11_000_000)]
    public async Task<ActionResult<TaskAttachmentItem>> UploadAttachment(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (!await db.Tasks.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)) return NotFound();
        if (file.Length <= 0 || file.Length > MaxAttachmentSize) return BadRequest(new { message = "Attachment size must be between 1 byte and 10 MB." });
        if (!AllowedAttachmentTypes.Contains(file.ContentType)) return BadRequest(new { message = "This file type is not allowed. Use PDF, image, text, CSV, Word, or Excel files." });
        var fileName = Path.GetFileName(file.FileName).Trim();
        if (string.IsNullOrWhiteSpace(fileName)) return BadRequest(new { message = "A valid file name is required." });
        if (fileName.Length > 180) fileName = fileName[..180];
        await using var stream = new MemoryStream(); await file.CopyToAsync(stream, cancellationToken);
        var attachment = new TaskAttachment { TaskItemId = id, FileName = fileName, ContentType = file.ContentType, Size = file.Length, Content = stream.ToArray(), UploadedBy = User.Identity?.Name ?? "system" };
        db.TaskAttachments.Add(attachment); db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = "AttachmentAdded", ActorReference = attachment.UploadedBy, ChangesJson = System.Text.Json.JsonSerializer.Serialize(new { attachment.FileName, attachment.Size }) }); await db.SaveChangesAsync(cancellationToken);
        return Ok(new TaskAttachmentItem(attachment.Id, attachment.FileName, attachment.ContentType, attachment.Size, attachment.UploadedBy, attachment.CreatedAt));
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = await db.TaskAttachments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == attachmentId && x.TaskItemId == id && !x.IsDeleted, cancellationToken);
        return attachment is null ? NotFound() : File(attachment.Content, attachment.ContentType, attachment.FileName);
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        var attachment = await db.TaskAttachments.FirstOrDefaultAsync(x => x.Id == attachmentId && x.TaskItemId == id && !x.IsDeleted, cancellationToken); if (attachment is null) return NotFound();
        attachment.IsDeleted = true; db.AuditEntries.Add(new AuditEntry { EntityName = nameof(TaskItem), EntityId = id.ToString(), Action = "AttachmentRemoved", ActorReference = User.Identity?.Name ?? "system", ChangesJson = System.Text.Json.JsonSerializer.Serialize(new { attachment.FileName }) }); await db.SaveChangesAsync(cancellationToken); return NoContent();
    }

    private Guid? CurrentUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private async Task<(Guid Id, string Name)?> CurrentIdentity(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return null;
        var user = await userManager.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId.Value && x.IsActive, cancellationToken);
        return user is null ? null : (user.Id, string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email ?? "Unknown user" : user.DisplayName);
    }

    private async Task<string?> ResolveOwnerName(Guid projectId, Guid? ownerUserId, CancellationToken cancellationToken)
    {
        if (!ownerUserId.HasValue) return null;
        var isProjectMember = await db.ProjectRoleAssignments.AnyAsync(x => x.ProjectId == projectId && x.UserId == ownerUserId.Value && !x.IsDeleted, cancellationToken);
        if (!isProjectMember) return null;
        var user = await userManager.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == ownerUserId.Value && x.IsActive, cancellationToken);
        return user is null ? null : string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;
    }

    private async Task<bool> CanEditTask(TaskItem task, Guid userId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrator")) return true;
        var roles = await db.ProjectRoleAssignments.AsNoTracking()
            .Where(x => x.ProjectId == task.ProjectId && x.UserId == userId && !x.IsDeleted)
            .Select(x => x.Role).ToListAsync(cancellationToken);
        return roles.Contains(ProjectRole.ProjectAdmin) || roles.Contains(ProjectRole.ProductOwner) || roles.Contains(ProjectRole.TeamLead)
            || (roles.Contains(ProjectRole.Requester) && task.ReporterUserId == userId)
            || (roles.Contains(ProjectRole.TeamMember) && task.OwnerUserId == userId);
    }

    private async Task<bool> CanManageAssignments(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrator")) return true;
        return await db.ProjectRoleAssignments.AsNoTracking().AnyAsync(x => x.ProjectId == projectId && x.UserId == userId &&
            (x.Role == ProjectRole.ProjectAdmin || x.Role == ProjectRole.ProductOwner || x.Role == ProjectRole.TeamLead), cancellationToken);
    }

    private async Task<bool> CanChangeOwner(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrator")) return true;
        return await db.ProjectRoleAssignments.AsNoTracking().AnyAsync(x => x.ProjectId == projectId && x.UserId == userId &&
            (x.Role == ProjectRole.ProjectAdmin || x.Role == ProjectRole.ProductOwner || x.Role == ProjectRole.TeamLead || x.Role == ProjectRole.Requester), cancellationToken);
    }
}
