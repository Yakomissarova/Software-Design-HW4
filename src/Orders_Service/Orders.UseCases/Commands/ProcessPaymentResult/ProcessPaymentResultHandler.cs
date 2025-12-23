using Microsoft.Extensions.Logging;
using Orders.Entities.Models;
using Orders.UseCases.Abstractions;

namespace Orders.UseCases.Commands.ProcessPaymentResult;

public class ProcessPaymentResultHandler
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ProcessPaymentResultHandler> _log;

    public ProcessPaymentResultHandler(
        IOrderRepository orders,
        IUnitOfWork uow,
        ILogger<ProcessPaymentResultHandler> log)
    {
        _orders = orders;
        _uow = uow;
        _log = log;
    }

    public async Task Handle(ProcessPaymentResultCommand cmd, CancellationToken ct)
    {
        _log.LogInformation("Orders ProcessPaymentResultHandler: received result OrderId={OrderId} Status={Status}",
            cmd.OrderId, cmd.Status);

        var order = await _orders.GetByIdAsync(cmd.OrderId, ct);
        if (order == null)
        {
            _log.LogWarning("Orders ProcessPaymentResultHandler: order NOT FOUND by Id={OrderId}", cmd.OrderId);
            return;
        }

        _log.LogInformation("Orders ProcessPaymentResultHandler: order found Id={Id} PublicId={PublicId} CurrentStatus={Status}",
            order.Id, order.PublicId, order.Status);

        if (order.Status != OrderStatus.New)
        {
            _log.LogInformation("Orders ProcessPaymentResultHandler: skip because order.Status={Status} (expected New)",
                order.Status);
            return;
        }

        if (cmd.Status == "Succeeded")
        {
            _log.LogInformation("Orders ProcessPaymentResultHandler: marking order finished Id={Id}", order.Id);
            order.MarkFinished();
        }
        else
        {
            _log.LogInformation("Orders ProcessPaymentResultHandler: marking order cancelled Id={Id}", order.Id);
            order.MarkCancelled();
        }

        await _uow.SaveChangesAsync(ct);

        _log.LogInformation("Orders ProcessPaymentResultHandler: saved changes for Id={Id}, newStatus={Status}",
            order.Id, order.Status);
    }
}
