using MediatR;

namespace Tasks.Application.Commands.CreateSubTask;

public record CreateSubTaskCommand(
    Guid ParentTaskId,
    string Title,
    string? Description,
    string Priority,
    Guid? AssigneeId,
    Guid CreatedByUserId,
    int? EstimatePoints) : IRequest<Guid>;
