namespace Payments.UseCases.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid MessageId,
    Guid OrderId,
    Guid UserId,
    decimal Amount
);