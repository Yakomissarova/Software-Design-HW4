using Orders.Infrastructure.Persistence;
using Orders.UseCases.Abstractions;

namespace Orders.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _db;
    public UnitOfWork(OrdersDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}