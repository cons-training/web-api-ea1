using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos
{
    public class DepositRequestDto
    {
        [Required]
        public string AccountNumber { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}