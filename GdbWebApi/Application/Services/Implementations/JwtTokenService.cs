using GdbWebApi.Application.Common.Security;
using GdbWebApi.Application.Services.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GdbWebApi.Application.Services.Implementations
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));

            if (string.IsNullOrWhiteSpace(_jwtOptions.Key) || _jwtOptions.Key.Length < 32)
            {
                throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long.");
            }
        }

        public string GenerateToken(string identifier, string name, string role, string? accountNumber = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, identifier),
                new Claim(ClaimTypes.NameIdentifier, identifier),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // If account number is present (customer token), add custom claim
            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                claims.Add(new Claim("accountNumber", accountNumber));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = GetTokenExpiry();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public DateTime GetTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);
        }
    }
}