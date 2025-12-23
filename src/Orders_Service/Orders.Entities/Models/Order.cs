namespace Orders.Entities.Models;

public class Order
{
    public Guid Id { get; private set; }

    // Читаемый идентификатор для пользователя (уникальный)
    public string PublicId { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Order() { } // EF

    public Order(Guid userId, decimal amount, string description)
    {
        if (userId == Guid.Empty)
            throw new InvalidOperationException("UserId is required");

        if (amount <= 0)
            throw new InvalidOperationException("Order amount must be positive");

        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidOperationException("Description is required");

        Id = Guid.NewGuid();
        PublicId = GeneratePublicId();
        UserId = userId;
        Amount = amount;
        Description = description.Trim();
        Status = OrderStatus.New;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFinished()
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Only new order can be finished");

        Status = OrderStatus.Finished;
    }

    public void MarkCancelled()
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Only new order can be cancelled");

        Status = OrderStatus.Cancelled;
    }

    private static string GeneratePublicId()
    {
        // ORD-20251223-123456
        // (читаемо; уникальность добьём UNIQUE в БД)
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = Random.Shared.Next(100000, 999999);
        return $"ORD-{date}-{suffix}";
    }
}