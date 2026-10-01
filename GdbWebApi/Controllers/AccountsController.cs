using Asp.Versioning;
using GDB.App.Application.Dtos;
using GDB.App.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Controllers;

/// <summary>
/// Manages bank accounts including account creation, balance inquiry, account lookup, and account closure.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsController"/> class.
    /// </summary>
    /// <param name="accountService">The business service handling account operations.</param>
    /// <param name="logger">The logger instance.</param>
    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new bank account.
    /// </summary>
    /// <param name="request">The account creation details including account type, initial balance, and holder details.</param>
    /// <returns>The newly created account information.</returns>
    /// <response code="201">Account created successfully.</response>
    /// <response code="400">Invalid account details provided or account already exists.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateAccountResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccountAsync([FromBody] CreateAccountRequestDto request)
    {
        try
        {
            var response = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(
                nameof(GetAccountByNumberAsync),
                new { accountNumber = response.AccountNumber, version = "1.0" },
                response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create account {AccountNumber}: {Message}", request.AccountNumber, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating account {AccountNumber}", request.AccountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves account details by account number.
    /// </summary>
    /// <param name="accountNumber">The unique 10-digit account number.</param>
    /// <returns>The account details if found.</returns>
    /// <response code="200">Account found and details returned.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewAccountResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountByNumberAsync(string accountNumber)
    {
        var account = await _accountService.ViewAccountAsync(accountNumber);
        return account == null
            ? NotFound(new { message = $"Account {accountNumber} not found." })
            : Ok(account);
    }

    /// <summary>
    /// Retrieves all accounts registered in the banking system.
    /// </summary>
    /// <returns>A collection of all account summaries.</returns>
    /// <response code="200">List of accounts returned successfully.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ViewAllAccountsResponseDto>))]
    public async Task<IActionResult> GetAllAccountsAsync()
    {
        var accounts = await Task.Run(() => _accountService.GetAllAccounts());
        return Ok(accounts);
    }

    /// <summary>
    /// Retrieves the current balance of an account.
    /// </summary>
    /// <param name="accountNumber">The unique 10-digit account number.</param>
    /// <returns>The account balance details.</returns>
    /// <response code="200">Account balance returned successfully.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("{accountNumber}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ViewBalanceResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountBalanceAsync(string accountNumber)
    {
        var balance = await _accountService.GetBalanceAsync(accountNumber);
        return balance == null
            ? NotFound(new { message = $"Account {accountNumber} not found." })
            : Ok(balance);
    }

    /// <summary>
    /// Closes an active bank account.
    /// </summary>
    /// <param name="accountNumber">The unique 10-digit account number to close.</param>
    /// <returns>Closure confirmation details.</returns>
    /// <response code="200">Account closed successfully.</response>
    /// <response code="400">Account is already closed or invalid operation.</response>
    /// <response code="404">Account not found.</response>
    [HttpDelete("{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CloseAccountResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseAccountAsync(string accountNumber)
    {
        try
        {
            var request = new CloseAccountRequestDto { AccountNumber = accountNumber };
            var response = await _accountService.CloseAccountAsync(request);
            return Ok(response);
        }
        catch (Exception ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Account closure failed: {AccountNumber} not found", accountNumber);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex.Message.Contains("already closed", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Account closure failed: {AccountNumber} already closed", accountNumber);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error closing account {AccountNumber}", accountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }
}
