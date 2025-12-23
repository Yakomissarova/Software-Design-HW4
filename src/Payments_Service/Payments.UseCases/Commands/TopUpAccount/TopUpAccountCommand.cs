namespace Payments.UseCases.Commands.TopUpAccount;

public record TopUpAccountCommand(Guid UserId, decimal Amount);