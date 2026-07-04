using MediatR;
using System.Text.Json;
using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Tasks.Domain.Events;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly ITaskRepository _tasks;
    private readonly IOutboxRepository _outbox;

    public CreateTaskCommandHandler(ITaskRepository tasks, IOutboxRepository outbox)
    {
        _tasks = tasks;
        _outbox = outbox;
    }

    public async System.Threading.Tasks.Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TaskItemPriority>(request.Priority, ignoreCase: true, out var priority))
            throw new ArgumentException($"Invalid priority value: '{request.Priority}'.");

        var (task, evt) = TaskItem.Create(
            request.Title,
            request.Description,
            priority,
            request.SprintId,
            request.AssigneeId,
            request.CreatedByUserId,
            request.EstimatePoints);

        await _tasks.AddAsync(task, cancellationToken);
        await _outbox.AddAsync(
            OutboxMessage.Create(nameof(TaskCreated), JsonSerializer.Serialize(evt)),
            cancellationToken);

        if (request.SprintId.HasValue)
            await _outbox.AddAsync(
                OutboxMessage.Create(nameof(TaskAddedToSprint), JsonSerializer.Serialize(new TaskAddedToSprint(task.Id, request.SprintId.Value))),
                cancellationToken);

        await _tasks.SaveChangesAsync(cancellationToken);
        return task.Id;
    }
}
