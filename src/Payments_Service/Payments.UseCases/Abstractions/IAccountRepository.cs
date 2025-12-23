using Payments.Entities.Models;

namespace Payments.UseCases.Abstractions;

public interface IAccountRepository
{
    Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<Account?> GetByLoginAsync(string login, CancellationToken ct);
    Task AddAsync(Account account, CancellationToken ct);
}