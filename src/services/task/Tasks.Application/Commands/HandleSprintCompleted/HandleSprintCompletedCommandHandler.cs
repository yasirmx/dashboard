using MediatR;
using System.Text.Json;
using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Tasks.Domain.Events;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.HandleSprintCompleted;

public sealed class HandleSprintCompletedCommandHandler : IRequestHandler<HandleSprintCompletedCommand>
{
    private readonly ITaskRepository _tasks;
    private readonly IOutboxRepository _outbox;

    public HandleSprintCompletedCommandHandler(ITaskRepository tasks, IOutboxRepository outbox)
    {
        _tasks = tasks;
        _outbox = outbox;
    }

    public async System.Threading.Tasks.Task Handle(HandleSprintCompletedCommand request, CancellationToken cancellationToken)
    {
        var unfinished = await _tasks.GetBySprintIdAsync(request.SprintId, cancellationToken);

        foreach (var task in unfinished.Where(t => t.Status != TaskItemStatus.Closed))
        {
            var (added, removed) = task.MoveToSprint(request.NextSprintId);

            if (removed is not null)
                await _outbox.AddAsync(
                    OutboxMessage.Create(nameof(TaskRemovedFromSprint), JsonSerializer.Serialize(removed)),
                    cancellationToken);

            if (added is not null)
                await _outbox.AddAsync(
                    OutboxMessage.Create(nameof(TaskAddedToSprint), JsonSerializer.Serialize(added)),
                    cancellationToken);
        }

        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
