using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskFlow.Application.Tasks;
using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

public sealed record AddTaskCommentRequest(string Body);

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(ITaskService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskListItem>>> List(
        [FromQuery] string? search,
        [FromQuery] WorkflowStatus? status,
        [FromQuery] Priority? priority,
        [FromQuery] Guid? projectId,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await service.ListAsync(search, status, priority, projectId, sortBy, sortDirection, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDetails>> Get(Guid id, CancellationToken cancellationToken) => (await service.GetAsync(id, cancellationToken)) is { } task ? Ok(task) : NotFound();

    [HttpPost]
    public async Task<ActionResult> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await service.CreateAsync(request, User.Identity?.Name ?? "system", cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDetails>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken) =>
        (await service.UpdateAsync(id, request, User.Identity?.Name ?? "system", cancellationToken)) is { } task ? Ok(task) : NotFound();

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] WorkflowStatus status, CancellationToken cancellationToken) => await service.ChangeStatusAsync(id, status, User.Identity?.Name ?? "system", cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<TaskCommentItem>> AddComment(Guid id, AddTaskCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await service.AddCommentAsync(id, request.Body, User.Identity?.Name ?? "system", cancellationToken);
        return comment is null ? BadRequest(new { message = "A valid task and comment body are required." }) : Ok(comment);
    }

    [HttpPost("{id:guid}/assignments")]
    public async Task<ActionResult<TaskAssignmentItem>> AddAssignment(Guid id, AddTaskAssignmentRequest request, CancellationToken cancellationToken)
    {
        var assignment = await service.AddAssignmentAsync(id, request, User.Identity?.Name ?? "system", cancellationToken);
        return assignment is null ? BadRequest(new { message = "A valid task and party reference are required." }) : Ok(assignment);
    }

    [HttpDelete("{id:guid}/assignments/{assignmentId:guid}")]
    public async Task<IActionResult> RemoveAssignment(Guid id, Guid assignmentId, CancellationToken cancellationToken) =>
        await service.RemoveAssignmentAsync(id, assignmentId, User.Identity?.Name ?? "system", cancellationToken) ? NoContent() : NotFound();
}
