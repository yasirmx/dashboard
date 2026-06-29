using MediatR;

namespace Authorization.Application.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest;
