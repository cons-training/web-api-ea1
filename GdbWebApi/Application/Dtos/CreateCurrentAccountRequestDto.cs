using GdbWebApi.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos
{
    public class CreateCurrentAccountRequestDto
    {
        [Required]
        public string AccountNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(18, 100)]
        public int Age { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string Pin { get; set; }

        public AccountPrivilege Privilege { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OverdraftLimit { get; set; }
    }
}