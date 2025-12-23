namespace Orders.Presentation.Contracts.Orders;

public record OrderDetailsResponse(
    string PublicId,
    Guid UserId,
    decimal Amount,
    string Description,
    string Status
);