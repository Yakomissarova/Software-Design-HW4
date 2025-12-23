using Microsoft.EntityFrameworkCore;
using Orders.Entities.Models;
using Orders.Infrastructure.Persistence;
using Orders.UseCases.Abstractions;

namespace Orders.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _db;
    public OrderRepository(OrdersDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Order?> GetByPublicIdAsync(string publicId, CancellationToken ct) =>
        _db.Orders.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);

    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {
        var items = await _db.Orders.ToListAsync(ct);
        return items
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var items = await _db.Orders
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        return items
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public Task AddAsync(Order order, CancellationToken ct) =>
        _db.Orders.AddAsync(order, ct).AsTask();
}