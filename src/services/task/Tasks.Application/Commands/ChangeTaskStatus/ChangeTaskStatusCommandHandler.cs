using MediatR;
using System.Text.Json;
using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Tasks.Domain.Events;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand>
{
    private readonly ITaskRepository _tasks;
    private readonly IOutboxRepository _outbox;

    public ChangeTaskStatusCommandHandler(ITaskRepository tasks, IOutboxRepository outbox)
    {
        _tasks = tasks;
        _outbox = outbox;
    }

    public async System.Threading.Tasks.Task Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TaskItemStatus>(request.Status, ignoreCase: true, out var newStatus))
            throw new ArgumentException($"Invalid status value: '{request.Status}'.");

        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task '{request.TaskId}' not found.");

        bool hasOpenSubTasks = false;
        if (newStatus == TaskItemStatus.Closed && task.ParentTaskId is null)
            hasOpenSubTasks = await _tasks.HasOpenSubTasksAsync(task.Id, cancellationToken);

        var evt = task.ChangeStatus(newStatus, hasOpenSubTasks);

        await _outbox.AddAsync(
            OutboxMessage.Create(nameof(TaskStatusChanged), JsonSerializer.Serialize(evt)),
            cancellationToken);

        await _tasks.SaveChangesAsync(cancellationToken);
    }
}
