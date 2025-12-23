using Microsoft.AspNetCore.Mvc;
using Payments.Presentation.Contracts.Accounts;
using Payments.UseCases.Abstractions;
using Payments.UseCases.Commands.CreateAccount;
using Payments.UseCases.Commands.TopUpAccount;
using Microsoft.AspNetCore.Http;

using Payments.UseCases.Queries.GetBalance;

namespace Payments.Presentation.Controllers;

[ApiController]
[Route("accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accounts;
    private readonly CreateAccountHandler _createAccount;
    private readonly TopUpAccountHandler _topUp;
    private readonly GetBalanceHandler _getBalance;

    public AccountsController(
        IAccountRepository accounts,
        CreateAccountHandler createAccount,
        TopUpAccountHandler topUp,
        GetBalanceHandler getBalance)
    {
        _accounts = accounts;
        _createAccount = createAccount;
        _topUp = topUp;
        _getBalance = getBalance;
    }

    /// <summary>Создать аккаунт (логин уникальный). UserId вычисляется детерминированно из логина.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var login = (request.Login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest("Login is required");

        Guid userId;
        try
        {
            userId = await _createAccount.Handle(new CreateAccountCommand(login), ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Login already exists"))
        {
            return Conflict("Login already exists");
        }

        var acc = await _accounts.GetByUserIdAsync(userId, ct);
        return Ok(new AccountResponse(acc!.UserId, acc.Login, acc.Balance));
    }

    /// <summary>Пополнить баланс по UserId.</summary>
    [HttpPost("topup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequest request, CancellationToken ct)
    {
        if (request.UserId == Guid.Empty)
            return BadRequest("UserId is required");
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        var acc = await _accounts.GetByUserIdAsync(request.UserId, ct);
        if (acc == null)
            return NotFound("Account not found");

        await _topUp.Handle(new TopUpAccountCommand(request.UserId, request.Amount), ct);
        return NoContent();
    }

    /// <summary>Пополнить баланс по логину.</summary>
    [HttpPost("topup-by-login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TopUpByLogin([FromBody] TopUpByLoginRequest request, CancellationToken ct)
    {
        var login = (request.Login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest("Login is required");
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        var acc = await _accounts.GetByLoginAsync(login, ct);
        if (acc == null)
            return NotFound("Account not found");

        await _topUp.Handle(new TopUpAccountCommand(acc.UserId, request.Amount), ct);
        return NoContent();
    }

    /// <summary>Получить аккаунт по логину.</summary>
    [HttpGet("by-login/{login}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLogin([FromRoute] string login, CancellationToken ct)
    {
        login = (login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest("login is required");

        var acc = await _accounts.GetByLoginAsync(login, ct);
        if (acc == null)
            return NotFound("Account not found");

        return Ok(new AccountResponse(acc.UserId, acc.Login, acc.Balance));
    }

    /// <summary>Получить баланс по UserId.</summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance([FromQuery] Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return BadRequest("userId is required");

        var acc = await _accounts.GetByUserIdAsync(userId, ct);
        if (acc == null)
            return NotFound("Account not found");

        var balance = await _getBalance.Handle(new GetBalanceQuery(userId), ct);
        return Ok(new BalanceResponse(acc.UserId, balance));
    }
}
