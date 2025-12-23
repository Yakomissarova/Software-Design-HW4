namespace Payments.Presentation.Contracts.Accounts;

public record TopUpByLoginRequest(string Login, decimal Amount);