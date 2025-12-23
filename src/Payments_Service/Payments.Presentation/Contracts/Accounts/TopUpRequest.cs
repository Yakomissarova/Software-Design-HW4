namespace Payments.Presentation.Contracts.Accounts;

public record TopUpRequest(Guid UserId, decimal Amount);