using Microsoft.EntityFrameworkCore;
using Payments.Entities.Models;
using Payments.Infrastructure.Persistence;
using Payments.UseCases.Abstractions;

namespace Payments.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly PaymentsDbContext _db;
    public AccountRepository(PaymentsDbContext db) => _db = db;

    public Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        _db.Accounts.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public Task<Account?> GetByLoginAsync(string login, CancellationToken ct) =>
        _db.Accounts.FirstOrDefaultAsync(x => x.Login == login, ct);

    public Task AddAsync(Account account, CancellationToken ct) =>
        _db.Accounts.AddAsync(account, ct).AsTask();
}