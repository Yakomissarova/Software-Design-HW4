namespace Orders.UseCases.Commands.ProcessPaymentResult;

public record ProcessPaymentResultCommand(
    Guid OrderId,
    string Status
);