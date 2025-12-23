using Payments.Entities.Models;

namespace Payments.UseCases.Abstractions;

public interface IInboxMessageRepository
{
    Task<bool> ExistsAsync(Guid messageId, CancellationToken ct);
    Task AddAsync(InboxMessage message, CancellationToken ct);
}