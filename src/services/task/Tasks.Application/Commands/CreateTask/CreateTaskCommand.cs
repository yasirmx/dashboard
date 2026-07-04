using MediatR;

namespace Tasks.Application.Commands.CreateTask;

public record CreateTaskCommand(
    string Title,
    string? Description,
    string Priority,
    Guid? SprintId,
    Guid? AssigneeId,
    Guid CreatedByUserId,
    int? EstimatePoints) : IRequest<Guid>;
