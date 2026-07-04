using MediatR;

namespace Tasks.Application.Commands.DeleteTask;

public record DeleteTaskCommand(Guid TaskId) : IRequest;
