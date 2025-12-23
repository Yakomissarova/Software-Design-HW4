namespace Orders.Presentation.Contracts.Orders;

public record CreateOrderResponse(
    string PublicId,
    string Status
);