using Payments.UseCases.Abstractions;

namespace Payments.UseCases.Queries.GetBalance;

public class GetBalanceHandler
{
    private readonly IAccountRepository _accounts;

    public GetBalanceHandler(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<decimal> Handle(GetBalanceQuery query, CancellationToken ct)
    {
        var account = await _accounts.GetByUserIdAsync(query.UserId, ct)
                      ?? throw new InvalidOperationException("Account not found");

        return account.Balance;
    }
}