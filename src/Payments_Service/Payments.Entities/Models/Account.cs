namespace Payments.Entities.Models;

public class Account
{
    public Guid UserId { get; private set; }

    // Уникальный логин пользователя
    public string Login { get; private set; } = null!;

    public decimal Balance { get; private set; }

    public Account(Guid userId, string login)
    {
        if (userId == Guid.Empty)
            throw new InvalidOperationException("UserId is required");

        if (string.IsNullOrWhiteSpace(login))
            throw new InvalidOperationException("Login is required");

        UserId = userId;
        Login = login.Trim();
        Balance = 0m;
    }

    public void TopUp(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Top up amount must be positive");

        Balance += amount;
    }

    public bool TryWithdraw(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Withdraw amount must be positive");

        if (Balance < amount)
            return false;

        Balance -= amount;
        return true;
    }
}