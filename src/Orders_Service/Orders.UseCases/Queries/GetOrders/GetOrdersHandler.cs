using Orders.Entities.Models;
using Orders.UseCases.Abstractions;

namespace Orders.UseCases.Queries.GetOrders;

public class GetOrdersHandler
{
    private readonly IOrderRepository _orders;

    public GetOrdersHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<IReadOnlyList<Order>> Handle(
        GetOrdersQuery query,
        CancellationToken ct)
    {
        return await _orders.GetByUserIdAsync(query.UserId, ct);
    }
}