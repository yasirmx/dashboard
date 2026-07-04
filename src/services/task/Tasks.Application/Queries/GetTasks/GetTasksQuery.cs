using MediatR;
using Tasks.Application.Models;

namespace Tasks.Application.Queries.GetTasks;

public record GetTasksQuery(
    Guid? SprintId,
    Guid? AssigneeId,
    string? Status,
    Guid? ParentId) : IRequest<IReadOnlyList<TaskDto>>;
