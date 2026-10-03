using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Domain.Enums;
using GdbWebApi.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController()
        {
            // Factory pattern retained intentionally for release 2
            _accountService = AccountServiceFactory.Create();
        }

        // GET: api/Account/{accNo}
        [HttpGet("{accNo}")]
        public async Task<IActionResult> GetAccountAsync(string accNo)
        {
            IAccount account = await _accountService.GetAccountAsync(accNo);

            if (account == null)
            {
                return NotFound(new { message = $"Account {accNo} not found." });
            }

            return Ok(account);
        }

        // GET: api/Account
        [HttpGet]
        public IActionResult GetAllAccounts()
        {
            List<ViewAllAccountsResponseDto> accounts =
                _accountService.GetAllAccounts();

            return Ok(accounts);
        }

        // GET: api/Account/{accNo}/balance
        [HttpGet("{accNo}/balance")]
        public async Task<IActionResult> GetBalanceAsync(string accNo)
        {
            try
            {
                ViewBalanceResponseDto response =
                    await _accountService.GetBalanceAsync(accNo);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/Account/{accNo}/details
        [HttpGet("{accNo}/details")]
        public async Task<IActionResult> ViewAccountAsync(string accNo)
        {
            try
            {
                ViewAccountResponseDto response =
                    await _accountService.ViewAccountAsync(accNo);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Account/savings
        [HttpPost("savings")]
        public IActionResult CreateSavingsAccount(
            [FromBody] CreateSavingsAccountRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.Savings,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                InterestRate = request.InterestRate,
                MinimumBalance = request.MinimumBalance
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/Account/current
        [HttpPost("current")]
        public IActionResult CreateCurrentAccount(
            [FromBody] CreateCurrentAccountRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.Current,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                OverdraftLimit = request.OverdraftLimit
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/Account/fixed-deposit
        [HttpPost("fixed-deposit")]
        public IActionResult CreateFixedDepositAccount(
            [FromBody] CreateFixedDepositAccountRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.FixedDeposit,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                TenureMonths = request.TenureMonths,
                InterestRate = request.InterestRate
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/Account/salary
        [HttpPost("salary")]
        public IActionResult CreateSalaryAccount(
            [FromBody] CreateSalaryAccountRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.Salary,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                EmployerName = request.EmployerName
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/Account
        [HttpPost]
        public IActionResult CreateAccount(
            [FromBody] CreateAccountRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            try
            {
                CreateAccountResponseDto response =
                    _accountService.CreateAccount(request);

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Account/{accountNumber}
        [HttpDelete("{accountNumber}")]
        public async Task<IActionResult> CloseAccountByRouteAsync(string accountNumber)
        {
            return await CloseAccountInternalAsync(accountNumber);
        }

        // PUT: api/Account/{accountNumber}/close
        [HttpPut("{accountNumber}/close")]
        public async Task<IActionResult> CloseAccountByRoutePutAsync(string accountNumber)
        {
            return await CloseAccountInternalAsync(accountNumber);
        }

        // PUT: api/Account/close
        [HttpPut("close")]
        public async Task<IActionResult> CloseAccountAsync(
            [FromBody] CloseAccountRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                return BadRequest(new { message = "Account number is required." });
            }

            return await CloseAccountInternalAsync(request.AccountNumber);
        }

        private async Task<IActionResult> CloseAccountInternalAsync(string accountNumber)
        {
            try
            {
                var request = new CloseAccountRequestDto { AccountNumber = accountNumber };
                CloseAccountResponseDto response =
                    await _accountService.CloseAccountAsync(request);

                return Ok(response);
            }
            catch (Exception ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("already closed", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}