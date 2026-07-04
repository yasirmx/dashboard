using MediatR;

namespace Tasks.Application.Commands.HandleSprintCancelled;

public record HandleSprintCancelledCommand(Guid SprintId) : IRequest;
