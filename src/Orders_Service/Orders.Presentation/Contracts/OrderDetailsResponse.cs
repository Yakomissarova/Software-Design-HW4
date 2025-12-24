namespace Orders.Presentation.Contracts;

public record OrderDetailsResponse(
    string PublicId,
    Guid UserId,
    decimal Amount,
    string Description,
    string Status
);