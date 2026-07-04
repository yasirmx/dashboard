using MediatR;

namespace Tasks.Application.Commands.AssignTask;

public record AssignTaskCommand(Guid TaskId, Guid AssigneeId) : IRequest;
