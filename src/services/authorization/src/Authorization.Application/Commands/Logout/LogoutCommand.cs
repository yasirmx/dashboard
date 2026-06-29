using MediatR;

namespace Authorization.Application.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
