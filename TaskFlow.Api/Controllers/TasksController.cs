using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskFlow.Application.Tasks;
using TaskFlow.Domain.Enums;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ITaskService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskListItem>>> List([FromQuery] string? search, [FromQuery] WorkflowStatus? status, CancellationToken cancellationToken) => Ok(await service.ListAsync(search, status, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) => (await service.GetAsync(id, cancellationToken)) is { } task ? Ok(task) : NotFound();

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await service.CreateAsync(request, User.Identity?.Name ?? "system", cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] WorkflowStatus status, CancellationToken cancellationToken) => await service.ChangeStatusAsync(id, status, User.Identity?.Name ?? "system", cancellationToken) ? NoContent() : NotFound();
}
