namespace Payments.Entities.Models;

public class PaymentOperation
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentOperationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public PaymentOperation(Guid orderId, Guid userId, decimal amount)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        UserId = userId;
        Amount = amount;
        Status = PaymentOperationStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSucceeded()
    {
        Status = PaymentOperationStatus.Succeeded;
    }

    public void MarkFailed()
    {
        Status = PaymentOperationStatus.Failed;
    }
}

public enum PaymentOperationStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}