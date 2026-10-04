using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos
{
    public class TransferRequestDto
    {
        [Required]
        public string FromAccount { get; set; }

        [Required]
        public string ToAccount { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string Pin { get; set; }
    }
}