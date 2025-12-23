using Orders.Entities.Models;

namespace Orders.UseCases.Abstractions;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct);
}