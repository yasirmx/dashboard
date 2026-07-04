using MediatR;

namespace Tasks.Application.Commands.ChangeTaskStatus;

public record ChangeTaskStatusCommand(Guid TaskId, string Status, Guid UserId) : IRequest;
