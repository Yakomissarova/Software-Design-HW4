using System.Text.Json;
using Orders.Entities.Models;
using Orders.UseCases.Abstractions;
using Orders.UseCases.Utils;

namespace Orders.UseCases.Commands.CreateOrder;

public class CreateOrderHandler
{
    private readonly IOrderRepository _orders;
    private readonly IOutboxMessageRepository _outbox;
    private readonly IUnitOfWork _uow;

    public CreateOrderHandler(IOrderRepository orders, IOutboxMessageRepository outbox, IUnitOfWork uow)
    {
        _orders = orders;
        _outbox = outbox;
        _uow = uow;
    }

    public async Task<string> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var login = (cmd.Login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login))
            throw new InvalidOperationException("Login is required");

        var userId = DeterministicGuid.FromLogin(login);

        var order = new Order(userId, cmd.Amount, cmd.Description);
        await _orders.AddAsync(order, ct);

        var message = new
        {
            MessageId = Guid.NewGuid(),
            OrderId = order.Id,
            UserId = userId,
            Login = login,
            Amount = order.Amount
        };

        await _outbox.AddAsync(
            new OutboxMessage("OrderPaymentRequested", JsonSerializer.Serialize(message)),
            ct);

        await _uow.SaveChangesAsync(ct);
        return order.PublicId;
    }
}