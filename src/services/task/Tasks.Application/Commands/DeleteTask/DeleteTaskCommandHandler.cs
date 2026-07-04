using MediatR;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly ITaskRepository _tasks;

    public DeleteTaskCommandHandler(ITaskRepository tasks) => _tasks = tasks;

    public async System.Threading.Tasks.Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task '{request.TaskId}' not found.");

        _tasks.Remove(task);
        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
