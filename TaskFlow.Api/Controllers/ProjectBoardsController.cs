using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using WorkflowStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Api.Controllers;

public sealed record BoardColumnItem(Guid Id, string Name, string Status, int SortOrder, int? WipLimit, bool IsDefaultDestination);
public sealed record ProjectBoardDetails(Guid ProjectId, IReadOnlyList<BoardColumnItem> Columns);
public sealed record SaveBoardColumnRequest(string Name, string Status, int SortOrder, int? WipLimit, bool IsDefaultDestination);
public sealed record SaveProjectBoardRequest(IReadOnlyList<SaveBoardColumnRequest> Columns);

[ApiController]
[Route("api/projects/{projectId:guid}/board-settings")]
[Authorize]
public sealed class ProjectBoardsController(IApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProjectBoardDetails>> Get(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await db.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken)) return NotFound();
            var workflowStatuses = await EffectiveWorkflowStatuses(projectId, cancellationToken);
            var board = await db.ProjectBoards.AsNoTracking().Include(x => x.Columns).SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);
            return Ok(new ProjectBoardDetails(projectId, MergeWithWorkflow(board?.Columns ?? [], workflowStatuses)));
        }
        catch (OperationCanceledException)
        {
            // Oracle reports a cancelled HTTP request as ORA-01013. Treat it as a
            // client-closed request instead of an unhandled API exception.
            return StatusCode(499);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ProjectBoardDetails>> Save(Guid projectId, SaveProjectBoardRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == projectId && !x.IsDeleted, cancellationToken)) return NotFound();
        if (request.Columns is null || request.Columns.Count == 0) return BadRequest(new { message = "Add at least one board column." });
        var workflowStatuses = await EffectiveWorkflowStatuses(projectId, cancellationToken);
        var parsed = new List<(SaveBoardColumnRequest Request, WorkflowStatus Status)>();
        foreach (var column in request.Columns)
        {
            if (string.IsNullOrWhiteSpace(column.Name)) return BadRequest(new { message = "Every board column requires a name." });
            if (!Enum.TryParse<WorkflowStatus>(column.Status, true, out var status)) return BadRequest(new { message = $"Unsupported status: {column.Status}." });
            if (column.WipLimit is <= 0) return BadRequest(new { message = "WIP limit must be greater than zero." });
            parsed.Add((column, status));
        }
        if (parsed.GroupBy(x => x.Status).Any(x => x.Count() > 1)) return BadRequest(new { message = "Each status can appear only once on a project board." });
        var configuredStatuses = parsed.Select(x => x.Status).ToHashSet();
        if (!configuredStatuses.SetEquals(workflowStatuses)) return BadRequest(new { message = "Board columns must match the statuses used by this project's workflow. Reload Board settings and try again." });

        var board = await db.ProjectBoards.SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);
        if (board is null) { board = new ProjectBoard { ProjectId = projectId }; db.ProjectBoards.Add(board); }
        else db.ProjectBoardColumns.RemoveRange(await db.ProjectBoardColumns.IgnoreQueryFilters().Where(x => x.ProjectBoardId == board.Id).ToListAsync(cancellationToken));
        foreach (var item in parsed.OrderBy(x => x.Request.SortOrder))
            db.ProjectBoardColumns.Add(new ProjectBoardColumn { ProjectBoardId = board.Id, ProjectBoard = board, Name = item.Request.Name.Trim(), Status = item.Status, SortOrder = item.Request.SortOrder, WipLimit = item.Request.WipLimit, IsDefaultDestination = item.Request.IsDefaultDestination });
        await db.SaveChangesAsync(cancellationToken);
        var saved = await db.ProjectBoardColumns.AsNoTracking().Where(x => x.ProjectBoardId == board.Id).ToListAsync(cancellationToken);
        return Ok(new ProjectBoardDetails(projectId, ToItems(saved)));
    }

    private async Task<IReadOnlyList<WorkflowStatus>> EffectiveWorkflowStatuses(Guid projectId, CancellationToken cancellationToken)
    {
        var schemeId = await db.WorkflowSchemes.Where(x => x.ProjectId == projectId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? await db.WorkflowSchemes.Where(x => x.IsDefault).Select(x => (Guid?)x.Id).SingleAsync(cancellationToken);
        var transitions = await db.WorkflowTransitions.AsNoTracking().Where(x => x.WorkflowSchemeId == schemeId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var statuses = new List<WorkflowStatus>();
        foreach (var transition in transitions)
        {
            if (!statuses.Contains(transition.FromStatus)) statuses.Add(transition.FromStatus);
            if (!statuses.Contains(transition.ToStatus)) statuses.Add(transition.ToStatus);
        }
        return statuses;
    }

    private static IReadOnlyList<BoardColumnItem> MergeWithWorkflow(IEnumerable<ProjectBoardColumn> savedColumns, IReadOnlyList<WorkflowStatus> workflowStatuses)
    {
        var allowed = workflowStatuses.ToHashSet();
        var result = savedColumns.Where(x => allowed.Contains(x.Status)).OrderBy(x => x.SortOrder)
            .Select(x => new BoardColumnItem(x.Id, x.Name, x.Status.ToString(), x.SortOrder, x.WipLimit, x.IsDefaultDestination)).ToList();
        var configured = result.Select(x => x.Status).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var status in workflowStatuses.Where(x => !configured.Contains(x.ToString())))
            result.Add(new BoardColumnItem(Guid.Empty, Label(status), status.ToString(), 0, null, true));
        return result.Select((x, index) => x with { SortOrder = (index + 1) * 10 }).ToArray();
    }

    private static IReadOnlyList<BoardColumnItem> ToItems(IEnumerable<ProjectBoardColumn> columns) => columns.OrderBy(x => x.SortOrder).Select(x => new BoardColumnItem(x.Id, x.Name, x.Status.ToString(), x.SortOrder, x.WipLimit, x.IsDefaultDestination)).ToArray();
    private static string Label(WorkflowStatus status) => System.Text.RegularExpressions.Regex.Replace(status.ToString(), "([a-z])([A-Z])", "$1 $2").Replace("Uat", "UAT");
}
