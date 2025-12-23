using Orders.Entities.Models;
using Orders.Infrastructure.Persistence;
using Orders.UseCases.Abstractions;

namespace Orders.Infrastructure.Repositories;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly OrdersDbContext _db;
    public OutboxMessageRepository(OrdersDbContext db) => _db = db;

    public Task AddAsync(OutboxMessage message, CancellationToken ct) =>
        _db.OutboxMessages.AddAsync(message, ct).AsTask();
}