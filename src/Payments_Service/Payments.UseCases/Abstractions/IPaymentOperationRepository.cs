using Payments.Entities.Models;

namespace Payments.UseCases.Abstractions;

public interface IPaymentOperationRepository
{
    Task<PaymentOperation?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);
    Task AddAsync(PaymentOperation operation, CancellationToken ct);
}