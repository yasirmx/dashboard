using Tasks.Application.Commands.HandleSprintCancelled;
using Tasks.Application.Commands.HandleSprintCompleted;
using Tasks.Infrastructure.Enums;
using Tasks.Infrastructure.Settings;
using Azure.Storage.Queues;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Tasks.Infrastructure.Workers;

public sealed class SprintEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly QueueClient _client;
    private readonly ILogger<SprintEventConsumer> _logger;

    public SprintEventConsumer(
        IOptions<QueueSettings> options,
        IServiceScopeFactory scopeFactory,
        ILogger<SprintEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var settings = options.Value;
        _client = new QueueClient(settings.ConnectionString, settings.ConsumerQueueName);
        _client.CreateIfNotExists();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SprintEventConsumer started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _client.ReceiveMessagesAsync(maxMessages: 10, cancellationToken: stoppingToken);

                foreach (var message in response.Value)
                {
                    await ProcessMessageAsync(message.MessageText, stoppingToken);
                    await _client.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming sprint events.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(string messageText, CancellationToken ct)
    {
        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(messageText));
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var eventTypeRaw = root.GetProperty("EventType").GetString() ?? string.Empty;
            var payload = root.GetProperty("Payload").GetString() ?? "{}";

            if (!Enum.TryParse<SprintEventType>(eventTypeRaw, ignoreCase: true, out var eventType))
            {
                _logger.LogDebug("Skipping unknown event type '{EventType}'", eventTypeRaw);
                return;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            switch (eventType)
            {
                case SprintEventType.SprintCompleted:
                {
                    using var payloadDoc = JsonDocument.Parse(payload);
                    var root2 = payloadDoc.RootElement;
                    var sprintId = root2.GetProperty("SprintId").GetGuid();
                    Guid? nextSprintId = root2.TryGetProperty("NextSprintId", out var nextProp) && nextProp.ValueKind == JsonValueKind.String
                        ? nextProp.GetGuid()
                        : null;
                    await mediator.Send(new HandleSprintCompletedCommand(sprintId, nextSprintId), ct);
                    _logger.LogInformation("Handled SprintCompleted for Sprint {SprintId} (rollover -> {NextSprintId})", sprintId, nextSprintId);
                    break;
                }
                case SprintEventType.SprintCancelled:
                {
                    using var payloadDoc = JsonDocument.Parse(payload);
                    var sprintId = payloadDoc.RootElement.GetProperty("SprintId").GetGuid();
                    await mediator.Send(new HandleSprintCancelledCommand(sprintId), ct);
                    _logger.LogInformation("Handled SprintCancelled for Sprint {SprintId}", sprintId);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process sprint event message.");
        }
    }
}
