namespace Payments.Presentation.Contracts.Accounts;

public record BalanceResponse(Guid UserId, decimal Balance);