using GdbWebApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos
{
    public class CreateAccountRequestDto
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

        public AccountType AccountType { get; set; }

        public AccountStatus Status { get; set; }

        public AccountPrivilege Privilege { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OverdraftLimit { get; set; }

        [Range(1, 120)]
        public int TenureMonths { get; set; }

        [Range(0, 100)]
        public double InterestRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinimumBalance { get; set; }

        public string EmployerName { get; set; }
    }
}
