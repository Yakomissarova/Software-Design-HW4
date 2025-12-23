namespace Orders.UseCases.Commands.CreateOrder;

public record CreateOrderCommand(string Login, decimal Amount, string Description);