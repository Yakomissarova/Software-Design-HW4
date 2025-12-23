namespace Orders.Presentation.Contracts.Orders;

public record OrderListItemResponse(
    string PublicId,
    decimal Amount,
    string Status
);