using GdbWebApi.Application.Common.Security;
using GdbWebApi.Application.Dtos.Auth;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Domain.Models;
using GdbWebApi.Infrastructure.Repositories.Contracts;

namespace GdbWebApi.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITokenService _tokenService;

        // Temporary hardcoded staff store for R6 (per Child Issue 12 requirement)
        private static readonly Dictionary<string, (string Password, string Role, string Name)> StaffStore =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["admin"] = ("Admin@123", UserRoles.Admin, "System Administrator"),
                ["manager"] = ("Manager@123", UserRoles.Manager, "Branch Manager"),
                ["teller"] = ("Teller@123", UserRoles.Teller, "Front Desk Teller")
            };

        public AuthService(IAccountRepository accountRepository, ITokenService tokenService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<AuthResponseDto?> AuthenticateAccountAsync(AccountLoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AccountNumber) || string.IsNullOrWhiteSpace(request.Pin))
            {
                return null;
            }

            // 1. Fetch account from repository
            IAccount account = await _accountRepository.GetAccountAsync(request.AccountNumber);
            if (account == null)
            {
                return null; // Account not found -> 401
            }

            // 2. Check if account is active
            if (!account.CheckIfAccountIsActive())
            {
                throw new InvalidOperationException("Account is inactive or closed.");
            }

            // 3. Validate PIN
            if (!account.ValidatePin(request.Pin))
            {
                return null; // Bad PIN -> 401
            }

            // 4. Generate JWT for customer with Role "User"
            string token = _tokenService.GenerateToken(
                identifier: account.AccountNumber,
                name: account.Name,
                role: UserRoles.User,
                accountNumber: account.AccountNumber);

            return new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                Role = UserRoles.User,
                AccountNumber = account.AccountNumber,
                Name = account.Name,
                ExpiresAt = _tokenService.GetTokenExpiry()
            };
        }

        public Task<AuthResponseDto?> AuthenticateStaffAsync(StaffLoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Task.FromResult<AuthResponseDto?>(null);
            }

            // Look up username in staff store
            if (StaffStore.TryGetValue(request.Username, out var staffInfo))
            {
                if (staffInfo.Password == request.Password)
                {
                    string token = _tokenService.GenerateToken(
                        identifier: request.Username,
                        name: staffInfo.Name,
                        role: staffInfo.Role);

                    return Task.FromResult<AuthResponseDto?>(new AuthResponseDto
                    {
                        Token = token,
                        TokenType = "Bearer",
                        Role = staffInfo.Role,
                        AccountNumber = null,
                        Name = staffInfo.Name,
                        ExpiresAt = _tokenService.GetTokenExpiry()
                    });
                }
            }

            return Task.FromResult<AuthResponseDto?>(null);
        }
    }
}