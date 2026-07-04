using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Application.Models;
using Tasks.Application.Queries.GetBoard;

namespace Tasks.Api.Controllers;

[ApiController]
[Route("sprints")]
[Authorize]
public class SprintsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SprintsController(IMediator mediator) => _mediator = mediator;

    // ── GET /sprints/{sprintId}/board ──────────────────────────────────────────
    [HttpGet("{sprintId:guid}/board")]
    [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoard(Guid sprintId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBoardQuery(sprintId), ct);
        return Ok(result);
    }
}
