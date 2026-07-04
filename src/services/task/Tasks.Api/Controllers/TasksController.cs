using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tasks.Application.Commands.AssignTask;
using Tasks.Application.Commands.ChangeTaskStatus;
using Tasks.Application.Commands.CreateSubTask;
using Tasks.Application.Commands.CreateTask;
using Tasks.Application.Commands.DeleteTask;
using Tasks.Application.Commands.MoveTaskToSprint;
using Tasks.Application.Commands.ReorderTask;
using Tasks.Application.Commands.UpdateTask;
using Tasks.Api.Requests;
using Tasks.Application.Models;
using Tasks.Application.Queries.GetBoard;
using Tasks.Application.Queries.GetSubTasks;
using Tasks.Application.Queries.GetTask;
using Tasks.Application.Queries.GetTasks;

namespace Tasks.Api.Controllers;

[ApiController]
[Route("tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // ── GET /tasks ─────────────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid? sprintId,
        [FromQuery] Guid? assigneeId,
        [FromQuery] string? status,
        [FromQuery] Guid? parentId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTasksQuery(sprintId, assigneeId, status, parentId), ct);
        return Ok(result);
    }

    // ── GET /tasks/{id} ────────────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid id, [FromQuery] bool includeSubTasks, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTaskQuery(id, includeSubTasks), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // ── POST /tasks ────────────────────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(
            new CreateTaskCommand(
                request.Title,
                request.Description,
                request.Priority,
                request.SprintId,
                request.AssigneeId,
                CurrentUserId,
                request.EstimatePoints),
            ct);

        return CreatedAtAction(nameof(GetTask), new { id }, new { id });
    }

    // ── PUT /tasks/{id} ────────────────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new UpdateTaskCommand(id, request.Title, request.Description, request.Priority, request.EstimatePoints),
            ct);
        return NoContent();
    }

    // ── DELETE /tasks/{id} ─────────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTaskCommand(id), ct);
        return NoContent();
    }

    // ── PATCH /tasks/{id}/status ───────────────────────────────────────────────
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ChangeTaskStatusCommand(id, request.Status, CurrentUserId), ct);
        return NoContent();
    }

    // ── PATCH /tasks/{id}/assign ───────────────────────────────────────────────
    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request, CancellationToken ct)
    {
        await _mediator.Send(new AssignTaskCommand(id, request.AssigneeId), ct);
        return NoContent();
    }

    // ── PATCH /tasks/{id}/sprint ───────────────────────────────────────────────
    [HttpPatch("{id:guid}/sprint")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveToSprint(Guid id, [FromBody] MoveToSprintRequest request, CancellationToken ct)
    {
        await _mediator.Send(new MoveTaskToSprintCommand(id, request.SprintId), ct);
        return NoContent();
    }

    // ── PATCH /tasks/{id}/order ────────────────────────────────────────────────
    [HttpPatch("{id:guid}/order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderTask(Guid id, [FromBody] ReorderTaskRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReorderTaskCommand(id, request.OrderIndex), ct);
        return NoContent();
    }

    // ── POST /tasks/{id}/subtasks ──────────────────────────────────────────────
    [HttpPost("{id:guid}/subtasks")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSubTask(Guid id, [FromBody] CreateSubTaskRequest request, CancellationToken ct)
    {
        var subTaskId = await _mediator.Send(
            new CreateSubTaskCommand(
                id,
                request.Title,
                request.Description,
                request.Priority,
                request.AssigneeId,
                CurrentUserId,
                request.EstimatePoints),
            ct);

        return CreatedAtAction(nameof(GetTask), new { id = subTaskId }, new { id = subTaskId });
    }

    // ── GET /tasks/{id}/subtasks ───────────────────────────────────────────────
    [HttpGet("{id:guid}/subtasks")]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubTasks(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubTasksQuery(id), ct);
        return Ok(result);
    }
}
