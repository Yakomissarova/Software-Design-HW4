using System.Text.Json;
using Payments.Entities.Models;
using Payments.UseCases.Abstractions;

namespace Payments.UseCases.Commands.ProcessPayment;

public class ProcessPaymentHandler
{
    private readonly IAccountRepository _accounts;
    private readonly IPaymentOperationRepository _payments;
    private readonly IInboxMessageRepository _inbox;
    private readonly IOutboxMessageRepository _outbox;
    private readonly IUnitOfWork _uow;

    public ProcessPaymentHandler(
        IAccountRepository accounts,
        IPaymentOperationRepository payments,
        IInboxMessageRepository inbox,
        IOutboxMessageRepository outbox,
        IUnitOfWork uow)
    {
        _accounts = accounts;
        _payments = payments;
        _inbox = inbox;
        _outbox = outbox;
        _uow = uow;
    }

    public async Task Handle(ProcessPaymentCommand cmd, CancellationToken ct)
    {
        if (await _inbox.ExistsAsync(cmd.MessageId, ct))
            return;

        await _inbox.AddAsync(
            new InboxMessage(cmd.MessageId, nameof(ProcessPaymentCommand), string.Empty),
            ct);

        // КРИТИЧНО: сохраняем inbox сразу, чтобы повторное получение сообщения не делало бесконечный ретрай
        await _uow.SaveChangesAsync(ct);

        if (await _payments.GetByOrderIdAsync(cmd.OrderId, ct) != null)
            return;

        var account = await _accounts.GetByUserIdAsync(cmd.UserId, ct);
        if (account == null)
        {
            // Вместо throw -> отправляем "Failed" в Orders
            var resultMessage = new { cmd.OrderId, Status = "Failed" };

            await _outbox.AddAsync(
                new OutboxMessage("OrderPaymentResult", JsonSerializer.Serialize(resultMessage)),
                ct);

            await _uow.SaveChangesAsync(ct);
            return;
        }

        var payment = new PaymentOperation(cmd.OrderId, cmd.UserId, cmd.Amount);

        if (account.TryWithdraw(cmd.Amount))
            payment.MarkSucceeded();
        else
            payment.MarkFailed();

        await _payments.AddAsync(payment, ct);

        var okResult = new { cmd.OrderId, Status = payment.Status.ToString() };

        await _outbox.AddAsync(
            new OutboxMessage("OrderPaymentResult", JsonSerializer.Serialize(okResult)),
            ct);

        await _uow.SaveChangesAsync(ct);
    }
}