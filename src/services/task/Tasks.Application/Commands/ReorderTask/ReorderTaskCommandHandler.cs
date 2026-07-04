using MediatR;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.ReorderTask;

public sealed class ReorderTaskCommandHandler : IRequestHandler<ReorderTaskCommand>
{
    private readonly ITaskRepository _tasks;

    public ReorderTaskCommandHandler(ITaskRepository tasks) => _tasks = tasks;

    public async System.Threading.Tasks.Task Handle(ReorderTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task '{request.TaskId}' not found.");

        task.Reorder(request.OrderIndex);
        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
