using Payments.Entities.Models;
using Payments.Infrastructure.Persistence;
using Payments.UseCases.Abstractions;

namespace Payments.Infrastructure.Repositories;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly PaymentsDbContext _db;
    public OutboxMessageRepository(PaymentsDbContext db) => _db = db;

    public Task AddAsync(OutboxMessage message, CancellationToken ct) =>
        _db.OutboxMessages.AddAsync(message, ct).AsTask();
}