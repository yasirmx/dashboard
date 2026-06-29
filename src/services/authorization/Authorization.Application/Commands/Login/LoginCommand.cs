using MediatR;

namespace Authorization.Application.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
