using Authorization.Application.Commands.ForgotPassword;
using Authorization.Application.Commands.Login;
using Authorization.Application.Commands.Logout;
using Authorization.Application.Commands.Refresh;
using Authorization.Application.Commands.Register;
using Authorization.Application.Commands.ResetPassword;
using Authorization.Application.Queries.Me;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    // POST /auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName), ct);
        return NoContent();
    }

    // POST /auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(new TokenResponse(result.AccessToken, result.RefreshToken));
    }

    // POST /auth/refresh
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshCommand(request.RefreshToken), ct);
        return Ok(new TokenResponse(result.AccessToken, result.RefreshToken));
    }

    // POST /auth/logout
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }

    // POST /auth/password/forgot
    [HttpPost("password/forgot")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email), ct);
        return NoContent();
    }

    // POST /auth/password/reset
    [HttpPost("password/reset")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), ct);
        return NoContent();
    }

    // GET /auth/me
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var rawToken = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var result = await _mediator.Send(new MeQuery(rawToken), ct);
        return Ok(new MeResponse(result.UserId, result.Email, result.Roles));
    }
}

// ── Request / Response DTOs ──────────────────────────────────────────────────

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record TokenResponse(string AccessToken, string RefreshToken);
public record MeResponse(Guid UserId, string Email, string[] Roles);
