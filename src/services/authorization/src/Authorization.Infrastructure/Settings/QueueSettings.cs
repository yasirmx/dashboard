namespace Authorization.Infrastructure.Settings;

public class QueueSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = "authorization";
}
