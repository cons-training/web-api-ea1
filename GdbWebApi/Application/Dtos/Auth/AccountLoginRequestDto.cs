using Microsoft.OpenApi.MicrosoftExtensions;
using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos.Auth
{
    public class AccountLoginRequestDto
    {
        [Required(ErrorMessage = "Account number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Account number must be exactly 10 digits.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "PIN is required.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits.")]
        public string Pin { get; set; } = string.Empty;
    }
}
