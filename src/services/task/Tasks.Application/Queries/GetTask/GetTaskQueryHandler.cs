using MediatR;
using Tasks.Application.Mappings;
using Tasks.Application.Models;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Queries.GetTask;

public sealed class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TaskDto?>
{
    private readonly ITaskRepository _tasks;

    public GetTaskQueryHandler(ITaskRepository tasks) => _tasks = tasks;

    public async System.Threading.Tasks.Task<TaskDto?> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        var task = request.IncludeSubTasks
            ? await _tasks.GetByIdWithSubTasksAsync(request.TaskId, cancellationToken)
            : await _tasks.GetByIdAsync(request.TaskId, cancellationToken);

        return task?.ToDto();
    }
}
