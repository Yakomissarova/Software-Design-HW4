using Payments.Entities.Models;

namespace Payments.UseCases.Abstractions;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct);
}