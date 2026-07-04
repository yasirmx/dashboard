using Tasks.Domain.Enums;
using Tasks.Domain.Events;

namespace Tasks.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public Guid? ParentTaskId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public TaskItemPriority Priority { get; private set; }
    public Guid? SprintId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public int? EstimatePoints { get; private set; }
    public int OrderIndex { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime UpdatedUtc { get; private set; }
    public DateTime? ClosedUtc { get; private set; }
    public byte[] RowVersion { get; set; } = [];

    // EF Core navigation properties
    public ICollection<TaskTag> Tags { get; set; } = [];
    public ICollection<TaskItem> SubTasks { get; set; } = [];

    private TaskItem() { }

    public static (TaskItem task, TaskCreated @event) Create(
        string title,
        string? description,
        TaskItemPriority priority,
        Guid? sprintId,
        Guid? assigneeId,
        Guid createdByUserId,
        int? estimatePoints,
        Guid? parentTaskId = null)
    {
        var item = new TaskItem
        {
            Id = Guid.NewGuid(),
            ParentTaskId = parentTaskId,
            Title = title,
            Description = description,
            Status = TaskItemStatus.New,
            Priority = priority,
            SprintId = sprintId,
            AssigneeId = assigneeId,
            CreatedByUserId = createdByUserId,
            EstimatePoints = estimatePoints,
            OrderIndex = 0,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        return (item, new TaskCreated(item.Id, sprintId, assigneeId, title));
    }

    public void Update(string title, string? description, TaskItemPriority priority, int? estimatePoints)
    {
        Title = title;
        Description = description;
        Priority = priority;
        EstimatePoints = estimatePoints;
        UpdatedUtc = DateTime.UtcNow;
    }

    public TaskStatusChanged ChangeStatus(TaskItemStatus newStatus, bool hasOpenSubTasks = false)
    {
        if (!IsValidTransition(Status, newStatus))
            throw new InvalidOperationException($"Invalid status transition from '{Status}' to '{newStatus}'.");

        if (newStatus == TaskItemStatus.Closed && hasOpenSubTasks)
            throw new InvalidOperationException("Cannot close a task that still has open sub-tasks.");

        var oldStatus = Status;
        Status = newStatus;
        UpdatedUtc = DateTime.UtcNow;

        if (newStatus == TaskItemStatus.Closed)
            ClosedUtc = DateTime.UtcNow;
        else if (oldStatus == TaskItemStatus.Closed)
            ClosedUtc = null;

        return new TaskStatusChanged(Id, SprintId, oldStatus.ToString(), newStatus.ToString());
    }

    public TaskAssigned Assign(Guid assigneeId)
    {
        AssigneeId = assigneeId;
        UpdatedUtc = DateTime.UtcNow;
        return new TaskAssigned(Id, assigneeId, Title);
    }

    public (TaskAddedToSprint? Added, TaskRemovedFromSprint? Removed) MoveToSprint(Guid? sprintId)
    {
        var oldSprintId = SprintId;
        SprintId = sprintId;
        UpdatedUtc = DateTime.UtcNow;

        var removed = oldSprintId.HasValue ? new TaskRemovedFromSprint(Id, oldSprintId.Value) : null;
        var added = sprintId.HasValue ? new TaskAddedToSprint(Id, sprintId.Value) : null;
        return (added, removed);
    }

    public void Reorder(int orderIndex)
    {
        OrderIndex = orderIndex;
        UpdatedUtc = DateTime.UtcNow;
    }

    public void DetachFromSprint()
    {
        SprintId = null;
        UpdatedUtc = DateTime.UtcNow;
    }

    private static bool IsValidTransition(TaskItemStatus from, TaskItemStatus to) =>
        (from, to) switch
        {
            (TaskItemStatus.New,     TaskItemStatus.Active)  => true,
            (TaskItemStatus.Active,  TaskItemStatus.Pending) => true,
            (TaskItemStatus.Active,  TaskItemStatus.Closed)  => true,
            (TaskItemStatus.Pending, TaskItemStatus.Active)  => true,
            (TaskItemStatus.Pending, TaskItemStatus.Closed)  => true,
            (TaskItemStatus.Closed,  TaskItemStatus.Active)  => true,   // reopen
            _ => false
        };
}
