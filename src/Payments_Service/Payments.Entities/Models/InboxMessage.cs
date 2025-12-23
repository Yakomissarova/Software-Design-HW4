namespace Payments.Entities.Models;

public class InboxMessage
{
    public Guid MessageId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private InboxMessage() { } // EF

    public InboxMessage(Guid messageId, string type, string payload)
    {
        MessageId = messageId;
        Type = type;
        Payload = payload;
        ReceivedAt = DateTimeOffset.UtcNow;
    }

    public void MarkProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}