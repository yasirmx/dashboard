using Authorization.Domain.Events;
using Authorization.Domain.Repositories;
using Authorization.Infrastructure.Settings;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Authorization.Infrastructure.Workers;

/// <summary>
/// Polls the Azure Queue for incoming <see cref="UserRolesChanged"/> events and
/// updates the local UserRoles copy so the next JWT reflects updated claims.
/// </summary>
public sealed class RolesChangedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly QueueClient _client;
    private readonly ILogger<RolesChangedConsumer> _logger;

    public RolesChangedConsumer(
        IOptions<QueueSettings> options,
        IServiceScopeFactory scopeFactory,
        ILogger<RolesChangedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var settings = options.Value;
        _client = new QueueClient(settings.ConnectionString, settings.QueueName);
        _client.CreateIfNotExists();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RolesChangedConsumer started.");

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
                _logger.LogError(ex, "Error consuming messages from queue.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(string messageText, CancellationToken ct)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(messageText));
            using var doc = JsonDocument.Parse(json);

            var eventName = doc.RootElement.GetProperty("EventName").GetString();
            if (eventName != "user.roles.changed") return;

            var payload = doc.RootElement.GetProperty("Payload").Deserialize<UserRolesChanged>();
            if (payload is null) return;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();

            await repo.ReplaceRolesAsync(payload.UserId, payload.Roles, ct);
            await repo.SaveChangesAsync(ct);

            _logger.LogInformation("Updated roles for UserId {UserId}", payload.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process queue message.");
        }
    }
}
