namespace Payments.Presentation.Contracts.Accounts;

public record AccountResponse(Guid UserId, string Login, decimal Balance);