namespace Authorization.Application.Abstractions;

public interface IQueuePublisher
{
    Task PublishAsync<T>(string eventName, T payload, CancellationToken ct = default);
}
