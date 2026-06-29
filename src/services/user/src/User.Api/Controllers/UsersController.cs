using MediatR;
using Microsoft.AspNetCore.Mvc;
using User.Application.Commands.Deactivate;
using User.Application.Commands.SetRoles;
using User.Application.Commands.UpdateProfile;
using User.Application.Models;
using User.Application.Queries.GetUser;
using User.Application.Queries.GetUsersByIds;
using User.Application.Queries.SearchUsers;

namespace User.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    // GET /users/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // GET /users?ids=a,b,c   (batch)  OR  GET /users?search=&role=  (list / filter)
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? ids,
        [FromQuery] string? search,
        [FromQuery] string? role,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(ids))
        {
            var parsed = ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();

            var byIds = await _mediator.Send(new GetUsersByIdsQuery(parsed), ct);
            return Ok(byIds);
        }

        var result = await _mediator.Send(new SearchUsersQuery(search, role), ct);
        return Ok(result);
    }

    // PUT /users/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        Guid id, [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new UpdateProfileCommand(id, request.FirstName, request.LastName, request.AvatarUrl, request.Department), ct);
        return NoContent();
    }

    // PUT /users/{id}/roles
    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRoles(
        Guid id, [FromBody] SetRolesRequest request, CancellationToken ct)
    {
        await _mediator.Send(new SetRolesCommand(id, request.Roles), ct);
        return NoContent();
    }

    // POST /users/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateUserCommand(id), ct);
        return NoContent();
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public record UpdateProfileRequest(string FirstName, string LastName, string? AvatarUrl, string? Department);
public record SetRolesRequest(string[] Roles);
