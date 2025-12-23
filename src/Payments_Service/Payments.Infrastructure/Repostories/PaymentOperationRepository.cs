using Microsoft.EntityFrameworkCore;
using Payments.Entities.Models;
using Payments.Infrastructure.Persistence;
using Payments.UseCases.Abstractions;

namespace Payments.Infrastructure.Repositories;

public class PaymentOperationRepository : IPaymentOperationRepository
{
    private readonly PaymentsDbContext _db;
    public PaymentOperationRepository(PaymentsDbContext db) => _db = db;

    public Task<PaymentOperation?> GetByOrderIdAsync(Guid orderId, CancellationToken ct) =>
        _db.PaymentOperations.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

    public Task AddAsync(PaymentOperation operation, CancellationToken ct) =>
        _db.PaymentOperations.AddAsync(operation, ct).AsTask();
}