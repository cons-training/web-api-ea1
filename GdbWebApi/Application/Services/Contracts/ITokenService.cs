using System.Security.Claims;
namespace GdbWebApi.Application.Services.Contracts
{
    public class ITokenService
    {
        string GenerateToken(string identifier, string name, string role, string? accountNumber = null);
        DateTime GetTokenExpiry();
    }
}
