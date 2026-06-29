using MediatR;

namespace User.Application.Commands.Deactivate;

public record DeactivateUserCommand(Guid UserId) : IRequest;
