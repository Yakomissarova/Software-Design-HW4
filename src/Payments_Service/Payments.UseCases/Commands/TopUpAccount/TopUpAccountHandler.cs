using Payments.UseCases.Abstractions;

namespace Payments.UseCases.Commands.TopUpAccount;

public class TopUpAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _uow;

    public TopUpAccountHandler(IAccountRepository accounts, IUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task Handle(TopUpAccountCommand cmd, CancellationToken ct)
    {
        var account = await _accounts.GetByUserIdAsync(cmd.UserId, ct)
                      ?? throw new InvalidOperationException("Account not found");

        account.TopUp(cmd.Amount);
        await _uow.SaveChangesAsync(ct);
    }
}