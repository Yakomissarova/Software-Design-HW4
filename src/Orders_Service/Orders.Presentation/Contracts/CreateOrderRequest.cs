namespace Orders.Presentation.Contracts.Orders;

public record CreateOrderRequest(
    string Login,
    decimal Amount,
    string Description
);