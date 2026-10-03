using System;

namespace GdbWebApi.Application.Dtos
{
    public class DepositRequestDto
    {
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
