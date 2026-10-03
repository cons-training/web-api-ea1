using Asp.Versioning;
using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using GdbWebApi.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Application.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionQueryService _transactionQueryService;

        // DI Constructor (used by ASP.NET Core)
        [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
        public TransactionController(
            ITransactionService transactionService,
            ITransactionQueryService transactionQueryService)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _transactionQueryService = transactionQueryService ?? throw new ArgumentNullException(nameof(transactionQueryService));
        }

        // Fallback parameterless constructor for Console UI (Home.cs)
        public TransactionController()
            : this(new TransactionService(), new TransactionQueryService())
        {
        }

        // POST: api/v1/Transaction/deposit
        [HttpPost("deposit")]
        public async Task<ActionResult<DepositResponseDto>> DepositAsync(
            [FromBody] DepositRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Deposit request cannot be null." });
            }

            try
            {
                var transactionDto = new TransactionDto
                {
                    AccountNumber = request.AccountNumber,
                    Amount = request.Amount
                };

                DepositResponseDto response =
                    await _transactionService
                        .ProcessTransactionAsync<DepositResponseDto>(
                            transactionDto,
                            TransactionType.Deposit);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/v1/Transaction/withdraw
        [HttpPost("withdraw")]
        public async Task<ActionResult<WithdrawResponseDto>> WithdrawAsync(
            [FromBody] WithdrawRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Withdraw request cannot be null." });
            }

            try
            {
                var transactionDto = new TransactionDto
                {
                    AccountNumber = request.AccountNumber,
                    Amount = request.Amount,
                    Pin = request.Pin
                };

                WithdrawResponseDto response =
                    await _transactionService
                        .ProcessTransactionAsync<WithdrawResponseDto>(
                            transactionDto,
                            TransactionType.Withdraw);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/v1/Transaction/transfer
        [HttpPost("transfer")]
        public async Task<ActionResult<TranferFundsResponseDto>> TransferFundsAsync(
            [FromBody] TransferRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Transfer request cannot be null." });
            }

            try
            {
                var transactionDto = new TransactionDto
                {
                    FromAccount = request.FromAccount,
                    ToAccount = request.ToAccount,
                    Amount = request.Amount,
                    Pin = request.Pin
                };

                TranferFundsResponseDto response =
                    await _transactionService
                        .ProcessTransactionAsync<TranferFundsResponseDto>(
                            transactionDto,
                            TransactionType.Transfer);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/v1/Transaction/{accountNumber}/recent
        [HttpGet("{accountNumber}/recent")]
        public async Task<ActionResult<List<ViewRecentTransactionsResponseDto>>>
            GetRecentTransactionsAsync(string accountNumber)
        {
            try
            {
                List<ViewRecentTransactionsResponseDto> response =
                    await _transactionQueryService
                        .GetRecentTransactionsAsync(accountNumber);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}