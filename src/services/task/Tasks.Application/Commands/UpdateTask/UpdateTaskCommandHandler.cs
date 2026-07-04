using MediatR;
using Tasks.Domain.Enums;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand>
{
    private readonly ITaskRepository _tasks;

    public UpdateTaskCommandHandler(ITaskRepository tasks) => _tasks = tasks;

    public async System.Threading.Tasks.Task Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TaskItemPriority>(request.Priority, ignoreCase: true, out var priority))
            throw new ArgumentException($"Invalid priority value: '{request.Priority}'.");

        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task '{request.TaskId}' not found.");

        task.Update(request.Title, request.Description, priority, request.EstimatePoints);
        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
