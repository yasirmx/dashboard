using MediatR;
using System.Text.Json;
using Tasks.Domain.Entities;
using Tasks.Domain.Events;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.MoveTaskToSprint;

public sealed class MoveTaskToSprintCommandHandler : IRequestHandler<MoveTaskToSprintCommand>
{
    private readonly ITaskRepository _tasks;
    private readonly IOutboxRepository _outbox;

    public MoveTaskToSprintCommandHandler(ITaskRepository tasks, IOutboxRepository outbox)
    {
        _tasks = tasks;
        _outbox = outbox;
    }

    public async System.Threading.Tasks.Task Handle(MoveTaskToSprintCommand request, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task '{request.TaskId}' not found.");

        var (added, removed) = task.MoveToSprint(request.SprintId);

        if (removed is not null)
            await _outbox.AddAsync(
                OutboxMessage.Create(nameof(TaskRemovedFromSprint), JsonSerializer.Serialize(removed)),
                cancellationToken);

        if (added is not null)
            await _outbox.AddAsync(
                OutboxMessage.Create(nameof(TaskAddedToSprint), JsonSerializer.Serialize(added)),
                cancellationToken);

        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
