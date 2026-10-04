using GdbWebApi.Application.Dtos.Auth;

namespace GdbWebApi.Application.Services.Contracts
{
    public class IAuthService
    {
        Task<AuthResponseDto?> AuthenticateAccountAsync(AccountLoginRequestDto request);
        Task<AuthResponseDto?> AuthenticateStaffAsync(StaffLoginRequestDto request);
    }
}
