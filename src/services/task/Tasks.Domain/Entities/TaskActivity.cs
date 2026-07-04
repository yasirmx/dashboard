namespace Tasks.Domain.Entities;

public class TaskActivity
{
    public Guid Id { get; private set; }
    public Guid TaskId { get; private set; }
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    private TaskActivity() { }

    public static TaskActivity Create(Guid taskId, Guid userId, string type, string? oldValue, string? newValue)
        => new()
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedUtc = DateTime.UtcNow
        };
}
