using Orders.Entities.Models;
using Orders.UseCases.Abstractions;

namespace Orders.UseCases.Queries.GetOrderById;

public class GetOrderByIdHandler
{
    private readonly IOrderRepository _orders;

    public GetOrderByIdHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<Order?> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var publicId = (query.PublicId).Trim();
        if (string.IsNullOrWhiteSpace(publicId))
            throw new InvalidOperationException("PublicId is required");

        return await _orders.GetByPublicIdAsync(publicId, ct);
    }
}