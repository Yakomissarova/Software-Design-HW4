namespace Payments.Presentation.Contracts.Accounts;

public record CreateAccountResponse(Guid UserId, string Login, decimal Balance);