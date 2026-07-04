using Tasks.Infrastructure.Settings;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Tasks.Infrastructure.Services;

public sealed class AzureQueuePublisher
{
    private readonly QueueClient _client;
    private readonly ILogger<AzureQueuePublisher> _logger;

    public AzureQueuePublisher(IOptions<QueueSettings> options, ILogger<AzureQueuePublisher> logger)
    {
        _logger = logger;
        var settings = options.Value;
        _client = new QueueClient(settings.ConnectionString, settings.PublisherQueueName);
        _client.CreateIfNotExists();
    }

    public async System.Threading.Tasks.Task PublishAsync(string eventType, string payload, CancellationToken ct)
    {
        var envelope = new { EventType = eventType, OccurredUtc = DateTime.UtcNow, Payload = payload };
        var json = JsonSerializer.Serialize(envelope);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        await _client.SendMessageAsync(base64, cancellationToken: ct);
        _logger.LogInformation("Published event '{EventType}'", eventType);
    }
}
