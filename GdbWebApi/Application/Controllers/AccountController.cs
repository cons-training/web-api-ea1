using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Domain.Models;
//using GdbWebApi.Application.Dtos;
//using GdbWebApi.Application.Services;
//using GdbWebApi.Application.Services.Contracts;
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
            ViewBalanceResponseDto response =
                await _accountService.GetBalanceAsync(accNo);

            return Ok(response);
        }

        // GET: api/Account/{accNo}/details
        [HttpGet("{accNo}/details")]
        public async Task<IActionResult> ViewAccountAsync(string accNo)
        {
            ViewAccountResponseDto response =
                await _accountService.ViewAccountAsync(accNo);

            return Ok(response);
        }

        // POST: api/Account
        [HttpPost]
        public IActionResult CreateAccount(
            [FromBody] CreateAccountRequestDto request)
        {
            CreateAccountResponseDto response =
                _accountService.CreateAccount(request);

            return Ok(response);
        }

        // PUT: api/Account/close
        [HttpPut("close")]
        public async Task<IActionResult> CloseAccountAsync(
            [FromBody] CloseAccountRequestDto request)
        {
            CloseAccountResponseDto response =
                await _accountService.CloseAccountAsync(request);

            return Ok(response);
        }
    }
}