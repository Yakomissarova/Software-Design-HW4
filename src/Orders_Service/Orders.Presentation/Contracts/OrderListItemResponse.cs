namespace Orders.Presentation.Contracts;

public record OrderListItemResponse(
    string PublicId,
    decimal Amount,
    string Status
);