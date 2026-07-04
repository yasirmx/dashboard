namespace Tasks.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredUtc { get; private set; }
    public DateTime? ProcessedUtc { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string payload)
        => new()
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            OccurredUtc = DateTime.UtcNow
        };

    public void MarkProcessed() => ProcessedUtc = DateTime.UtcNow;
}
