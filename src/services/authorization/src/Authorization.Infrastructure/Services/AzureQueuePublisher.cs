using Authorization.Application.Abstractions;
using Authorization.Infrastructure.Settings;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Authorization.Infrastructure.Services;

public sealed class AzureQueuePublisher : IQueuePublisher
{
    private readonly QueueClient _client;
    private readonly ILogger<AzureQueuePublisher> _logger;

    public AzureQueuePublisher(IOptions<QueueSettings> options, ILogger<AzureQueuePublisher> logger)
    {
        _logger = logger;
        var settings = options.Value;
        _client = new QueueClient(settings.ConnectionString, settings.QueueName);
        _client.CreateIfNotExists();
    }

    public async Task PublishAsync<T>(string eventName, T payload, CancellationToken ct)
    {
        var envelope = new { EventName = eventName, OccurredUtc = DateTime.UtcNow, Payload = payload };
        var json = JsonSerializer.Serialize(envelope);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        await _client.SendMessageAsync(base64, cancellationToken: ct);
        _logger.LogInformation("Published event '{EventName}'", eventName);
    }
}
