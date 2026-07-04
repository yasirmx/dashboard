using MediatR;

namespace Tasks.Application.Commands.UpdateTask;

public record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description,
    string Priority,
    int? EstimatePoints) : IRequest;
