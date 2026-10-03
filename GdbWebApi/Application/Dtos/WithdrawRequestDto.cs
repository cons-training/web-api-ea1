using System;

namespace GdbWebApi.Application.Dtos
{
    public class WithdrawRequestDto
    {
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Pin { get; set; }
    }
}
