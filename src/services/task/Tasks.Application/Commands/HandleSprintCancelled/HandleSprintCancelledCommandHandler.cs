using MediatR;
using System.Text.Json;
using Tasks.Domain.Entities;
using Tasks.Domain.Events;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.HandleSprintCancelled;

public sealed class HandleSprintCancelledCommandHandler : IRequestHandler<HandleSprintCancelledCommand>
{
    private readonly ITaskRepository _tasks;
    private readonly IOutboxRepository _outbox;

    public HandleSprintCancelledCommandHandler(ITaskRepository tasks, IOutboxRepository outbox)
    {
        _tasks = tasks;
        _outbox = outbox;
    }

    public async System.Threading.Tasks.Task Handle(HandleSprintCancelledCommand request, CancellationToken cancellationToken)
    {
        var allTasks = await _tasks.GetBySprintIdAsync(request.SprintId, cancellationToken);

        foreach (var task in allTasks)
        {
            task.DetachFromSprint();
            await _outbox.AddAsync(
                OutboxMessage.Create(nameof(TaskRemovedFromSprint),
                    JsonSerializer.Serialize(new TaskRemovedFromSprint(task.Id, request.SprintId))),
                cancellationToken);
        }

        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
