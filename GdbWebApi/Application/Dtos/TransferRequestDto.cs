using System;

namespace GdbWebApi.Application.Dtos
{
    public class TransferRequestDto
    {
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public decimal Amount { get; set; }
        public string Pin { get; set; }
    }
}
