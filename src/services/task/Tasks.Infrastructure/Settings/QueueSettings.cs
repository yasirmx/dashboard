namespace Tasks.Infrastructure.Settings;

public class QueueSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string PublisherQueueName { get; set; } = "task-events";
    public string ConsumerQueueName { get; set; } = "sprint-events";
}
