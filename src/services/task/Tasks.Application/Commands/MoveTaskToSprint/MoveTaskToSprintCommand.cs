using MediatR;

namespace Tasks.Application.Commands.MoveTaskToSprint;

public record MoveTaskToSprintCommand(Guid TaskId, Guid? SprintId) : IRequest;
