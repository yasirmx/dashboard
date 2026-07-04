using MediatR;
using System.Text.Json;
using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Tasks.Domain.Events;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Commands.CreateSubTask;

public sealed class CreateSubTaskCommandHandler : IRequestHandler<CreateSubTaskCommand, Guid>
{
    private readonly ITaskRepository _tasks;
    private readonly IOutboxRepository _outbox;

    public CreateSubTaskCommandHandler(ITaskRepository tasks, IOutboxRepository outbox)
    {
        _tasks = tasks;
        _outbox = outbox;
    }

    public async System.Threading.Tasks.Task<Guid> Handle(CreateSubTaskCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TaskItemPriority>(request.Priority, ignoreCase: true, out var priority))
            throw new ArgumentException($"Invalid priority value: '{request.Priority}'.");

        var parent = await _tasks.GetByIdAsync(request.ParentTaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Parent task '{request.ParentTaskId}' not found.");

        if (parent.ParentTaskId.HasValue)
            throw new InvalidOperationException("Cannot create a sub-task under a sub-task (max depth is 1).");

        var (subTask, evt) = TaskItem.Create(
            request.Title,
            request.Description,
            priority,
            parent.SprintId,        // inherit sprint from parent
            request.AssigneeId,
            request.CreatedByUserId,
            request.EstimatePoints,
            parentTaskId: request.ParentTaskId);

        await _tasks.AddAsync(subTask, cancellationToken);
        await _outbox.AddAsync(
            OutboxMessage.Create(nameof(TaskCreated), JsonSerializer.Serialize(evt)),
            cancellationToken);

        await _tasks.SaveChangesAsync(cancellationToken);
        return subTask.Id;
    }
}
