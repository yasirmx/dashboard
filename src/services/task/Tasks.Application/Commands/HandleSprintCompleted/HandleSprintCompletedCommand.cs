using MediatR;

namespace Tasks.Application.Commands.HandleSprintCompleted;

public record HandleSprintCompletedCommand(Guid SprintId, Guid? NextSprintId) : IRequest;
