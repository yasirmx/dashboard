namespace Tasks.Domain.Entities;

public class TaskTag
{
    public Guid TaskId { get; private set; }
    public string Tag { get; private set; } = string.Empty;

    private TaskTag() { }

    public static TaskTag Create(Guid taskId, string tag)
        => new() { TaskId = taskId, Tag = tag.ToLowerInvariant().Trim() };
}
