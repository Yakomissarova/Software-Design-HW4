using Orders.Entities.Models;

namespace Orders.UseCases.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Order?> GetByPublicIdAsync(string publicId, CancellationToken ct);

    Task<List<Order>> GetAllAsync(CancellationToken ct);

    Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct);

    Task AddAsync(Order order, CancellationToken ct);
}