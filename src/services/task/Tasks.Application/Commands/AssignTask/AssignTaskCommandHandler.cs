using MediatR;
using System.Text.Json;
using Tasks.Domain.Entities;
using Tasks.Domain.Events;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.AssignTask;

public sealed class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand>
{
    private readonly ITaskRepository _tasks;
    private readonly IOutboxRepository _outbox;

    public AssignTaskCommandHandler(ITaskRepository tasks, IOutboxRepository outbox)
    {
        _tasks = tasks;
        _outbox = outbox;
    }

    public async System.Threading.Tasks.Task Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task '{request.TaskId}' not found.");

        var evt = task.Assign(request.AssigneeId);

        await _outbox.AddAsync(
            OutboxMessage.Create(nameof(TaskAssigned), JsonSerializer.Serialize(evt)),
            cancellationToken);

        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
