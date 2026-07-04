using MediatR;
using Tasks.Application.Mappings;
using Tasks.Application.Models;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Queries.GetSubTasks;

public sealed class GetSubTasksQueryHandler : IRequestHandler<GetSubTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _tasks;

    public GetSubTasksQueryHandler(ITaskRepository tasks) => _tasks = tasks;

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskDto>> Handle(GetSubTasksQuery request, CancellationToken cancellationToken)
    {
        var subTasks = await _tasks.GetSubTasksAsync(request.ParentTaskId, cancellationToken);
        return subTasks.Select(t => t.ToDto()).ToList();
    }
}
