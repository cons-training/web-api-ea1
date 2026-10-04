namespace GdbWebApi.Application.Dtos.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public string Role { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
