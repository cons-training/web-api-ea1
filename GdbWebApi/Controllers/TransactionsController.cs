using Asp.Versioning;
using GDB.App.Application.Dtos;
using GDB.App.Application.Services.Contracts;
using GDB.App.Domain.Enums;
using GDB.App.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Controllers;

/// <summary>
/// Manages financial transactions including deposits, withdrawals, fund transfers, and transaction history inquiries.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ITransactionQueryService _transactionQueryService;
    private readonly ILogger<TransactionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionsController"/> class.
    /// </summary>
    /// <param name="transactionService">The business service for processing transactions.</param>
    /// <param name="transactionQueryService">The query service for transaction history.</param>
    /// <param name="logger">The logger instance.</param>
    public TransactionsController(
        ITransactionService transactionService,
        ITransactionQueryService transactionQueryService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _transactionQueryService = transactionQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Deposits funds into a specified bank account.
    /// </summary>
    /// <param name="request">The deposit details containing the account number and amount.</param>
    /// <returns>The result of the deposit operation with the updated balance.</returns>
    /// <response code="200">Deposit completed successfully.</response>
    /// <response code="400">Invalid amount or inactive account.</response>
    /// <response code="404">Account not found.</response>
    [HttpPost("deposit")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepositResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DepositAsync([FromBody] TransactionDto request)
    {
        try
        {
            var response = await _transactionService
                .ProcessTransactionAsync<DepositResponseDto>(request, TransactionType.Deposit);
            return Ok(response);
        }
        catch (InvalidAmountException ex)
        {
            _logger.LogWarning(ex, "Deposit failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InactiveAccountException ex)
        {
            _logger.LogWarning(ex, "Deposit failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (AccountException ex)
        {
            _logger.LogWarning(ex, "Deposit failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during deposit for account {AccountNumber}", request?.AccountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Withdraws funds from a specified bank account after validating PIN and balance.
    /// </summary>
    /// <param name="request">The withdrawal details containing account number, PIN, and amount.</param>
    /// <returns>The result of the withdrawal operation with the updated balance.</returns>
    /// <response code="200">Withdrawal completed successfully.</response>
    /// <response code="400">Invalid withdrawal request or inactive account.</response>
    /// <response code="401">Invalid PIN provided.</response>
    /// <response code="404">Account not found.</response>
    /// <response code="422">Insufficient balance or minimum balance violation.</response>
    [HttpPost("withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WithdrawResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> WithdrawAsync([FromBody] TransactionDto request)
    {
        try
        {
            var response = await _transactionService
                .ProcessTransactionAsync<WithdrawResponseDto>(request, TransactionType.Withdraw);
            return Ok(response);
        }
        catch (InvalidPinException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed: Invalid PIN for account {AccountNumber}", request?.AccountNumber);
            return StatusCode(StatusCodes.Status401Unauthorized, new { message = ex.Message });
        }
        catch (InsufficientBalanceException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed: Insufficient balance for account {AccountNumber}", request?.AccountNumber);
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
        }
        catch (MinimumBalanceViolationException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed: Minimum balance violation for account {AccountNumber}", request?.AccountNumber);
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
        }
        catch (InvalidAmountException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InactiveAccountException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (AccountException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during withdrawal for account {AccountNumber}", request?.AccountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Transfers funds between two bank accounts.
    /// </summary>
    /// <param name="request">The transfer details containing sender account, receiver account, sender PIN, and amount.</param>
    /// <returns>The result of the fund transfer operation.</returns>
    /// <response code="200">Transfer completed successfully.</response>
    /// <response code="400">Invalid transfer request, same account transfer, or inactive account.</response>
    /// <response code="401">Invalid PIN provided.</response>
    /// <response code="404">Source or destination account not found.</response>
    /// <response code="422">Insufficient balance or minimum balance violation.</response>
    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TranferFundsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> TransferFundsAsync([FromBody] TransactionDto request)
    {
        try
        {
            var response = await _transactionService
                .ProcessTransactionAsync<TranferFundsResponseDto>(request, TransactionType.Transfer);
            return Ok(response);
        }
        catch (InvalidPinException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: Invalid PIN for account {FromAccount}", request?.FromAccount);
            return StatusCode(StatusCodes.Status401Unauthorized, new { message = ex.Message });
        }
        catch (InsufficientBalanceException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: Insufficient balance for account {FromAccount}", request?.FromAccount);
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
        }
        catch (MinimumBalanceViolationException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: Minimum balance violation for account {FromAccount}", request?.FromAccount);
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
        }
        catch (InvalidAmountException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InactiveAccountException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (AccountException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Transfer failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (AccountException ex)
        {
            _logger.LogWarning(ex, "Transfer failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during transfer from {FromAccount} to {ToAccount}", request?.FromAccount, request?.ToAccount);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves recent transaction history for a specified account.
    /// </summary>
    /// <param name="accountNumber">The unique 10-digit account number.</param>
    /// <returns>A collection of recent transaction records.</returns>
    /// <response code="200">Transaction history returned successfully.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("{accountNumber}/recent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ViewRecentTransactionsResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecentTransactionsAsync(string accountNumber)
    {
        try
        {
            var transactions = await _transactionQueryService.GetRecentTransactionsAsync(accountNumber);
            return Ok(transactions);
        }
        catch (AccountException ex)
        {
            _logger.LogWarning(ex, "Recent transactions failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving recent transactions for account {AccountNumber}", accountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }
}
