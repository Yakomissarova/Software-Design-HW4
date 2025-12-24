namespace Payments.Entities.Models;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public OutboxMessageStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public OutboxMessage(string type, string payload)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        Status = OutboxMessageStatus.Pending;
        Attempts = 0;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSent()
    {
        Status = OutboxMessageStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementAttempts()
    {
        Attempts++;
    }
}

public enum OutboxMessageStatus
{
    Pending = 0,
    Processed = 1
}