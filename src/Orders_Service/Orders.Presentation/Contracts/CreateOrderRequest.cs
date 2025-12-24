namespace Orders.Presentation.Contracts;

public record CreateOrderRequest(
    string Login,
    decimal Amount,
    string Description
);