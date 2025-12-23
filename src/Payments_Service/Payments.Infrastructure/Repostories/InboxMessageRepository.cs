using Microsoft.EntityFrameworkCore;
using Payments.Entities.Models;
using Payments.Infrastructure.Persistence;
using Payments.UseCases.Abstractions;

namespace Payments.Infrastructure.Repositories;

public class InboxMessageRepository : IInboxMessageRepository
{
    private readonly PaymentsDbContext _db;
    public InboxMessageRepository(PaymentsDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid messageId, CancellationToken ct) =>
        _db.InboxMessages.AnyAsync(x => x.MessageId == messageId, ct);

    public Task AddAsync(InboxMessage message, CancellationToken ct) =>
        _db.InboxMessages.AddAsync(message, ct).AsTask();
}