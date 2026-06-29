namespace User.Infrastructure.Settings;

public class QueueSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Queue this service publishes outbound events to.</summary>
    public string PublisherQueueName { get; set; } = "users";

    /// <summary>Queue this service consumes inbound events from.</summary>
    public string ConsumerQueueName { get; set; } = "authorization";
}
