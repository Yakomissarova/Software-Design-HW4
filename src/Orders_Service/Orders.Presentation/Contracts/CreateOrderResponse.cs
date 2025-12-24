namespace Orders.Presentation.Contracts;

public record CreateOrderResponse(
    string PublicId,
    string Status
);