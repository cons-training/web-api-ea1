using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos
{
    public class WithdrawRequestDto
    {
        [Required]
        public string AccountNumber { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string Pin { get; set; }
    }
}