using MediatR;
using Tasks.Application.Mappings;
using Tasks.Application.Models;
using Tasks.Domain.Enums;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Queries.GetTasks;

public sealed class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _tasks;

    public GetTasksQueryHandler(ITaskRepository tasks) => _tasks = tasks;

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        TaskItemStatus? status = null;
        if (request.Status is not null && Enum.TryParse<TaskItemStatus>(request.Status, ignoreCase: true, out var parsed))
            status = parsed;

        var tasks = await _tasks.GetAsync(request.SprintId, request.AssigneeId, status, request.ParentId, cancellationToken);
        return tasks.Select(t => t.ToDto()).ToList();
    }
}
