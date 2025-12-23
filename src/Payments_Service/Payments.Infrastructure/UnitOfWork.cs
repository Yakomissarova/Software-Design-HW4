using Payments.Infrastructure.Persistence;
using Payments.UseCases.Abstractions;

namespace Payments.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentsDbContext _db;
    public UnitOfWork(PaymentsDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}