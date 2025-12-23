using Payments.Entities.Models;
using Payments.UseCases.Abstractions;
using Payments.UseCases.Utils;

namespace Payments.UseCases.Commands.CreateAccount;

public class CreateAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _uow;

    public CreateAccountHandler(IAccountRepository accounts, IUnitOfWork uow)
    {
        _accounts = accounts;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateAccountCommand cmd, CancellationToken ct)
    {
        var login = (cmd.Login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login))
            throw new InvalidOperationException("Login is required");

        var userId = DeterministicGuid.FromLogin(login);

        // идемпотентность: если уже есть по userId — просто вернём его
        var existingById = await _accounts.GetByUserIdAsync(userId, ct);
        if (existingById != null)
            return existingById.UserId;

        // уникальность логина
        var existingByLogin = await _accounts.GetByLoginAsync(login, ct);
        if (existingByLogin != null)
            throw new InvalidOperationException("Login already exists");

        await _accounts.AddAsync(new Account(userId, login), ct);
        await _uow.SaveChangesAsync(ct);

        return userId;
    }
}