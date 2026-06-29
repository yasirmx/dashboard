using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using User.Application.Abstractions;
using User.Domain.Entities;
using User.Domain.Events;
using User.Domain.Repositories;
using User.Infrastructure.Settings;

namespace User.Infrastructure.Workers;

/// <summary>
/// Polls the inbound queue for <see cref="UserRegistered"/> events and reactively creates
/// the matching profile. Idempotent (upsert by UserId) so duplicate events are tolerated.
/// </summary>
public sealed class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IQueuePublisher _publisher;
    private readonly QueueClient _client;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(
        IOptions<QueueSettings> options,
        IServiceScopeFactory scopeFactory,
        IQueuePublisher publisher,
        ILogger<UserRegisteredConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
        var settings = options.Value;
        _client = new QueueClient(settings.ConnectionString, settings.ConsumerQueueName);
        _client.CreateIfNotExists();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserRegisteredConsumer started.");

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
            if (eventName != "user.registered") return;

            var payload = doc.RootElement.GetProperty("Payload").Deserialize<UserRegistered>();
            if (payload is null) return;

            using var scope = _scopeFactory.CreateScope();
            var profiles = scope.ServiceProvider.GetRequiredService<IProfileRepository>();
            var roles = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();

            // Idempotent upsert: skip if the profile already exists.
            var existing = await profiles.GetByIdAsync(payload.UserId, ct);
            if (existing is not null)
            {
                _logger.LogInformation("Profile {UserId} already exists; ignoring duplicate.", payload.UserId);
                return;
            }

            var profile = Profile.Create(payload.UserId, payload.Email, payload.FirstName, payload.LastName);
            await profiles.AddAsync(profile, ct);
            await profiles.SaveChangesAsync(ct);

            // Assign the default role and publish the change so Auth can rebuild JWT claims.
            await roles.ReplaceRolesAsync(payload.UserId, [Roles.Dev], ct);
            await roles.SaveChangesAsync(ct);

            await _publisher.PublishAsync(
                "user.roles.changed", new UserRolesChanged(payload.UserId, [Roles.Dev]), ct);

            _logger.LogInformation("Created profile {UserId} with default role.", payload.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process queue message.");
        }
    }
}
