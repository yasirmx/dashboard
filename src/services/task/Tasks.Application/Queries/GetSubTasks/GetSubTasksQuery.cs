using MediatR;
using Tasks.Application.Models;

namespace Tasks.Application.Queries.GetSubTasks;

public record GetSubTasksQuery(Guid ParentTaskId) : IRequest<IReadOnlyList<TaskDto>>;
