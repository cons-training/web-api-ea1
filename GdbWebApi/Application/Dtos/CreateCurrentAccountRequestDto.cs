using GdbWebApi.Domain.Enums;

namespace GdbWebApi.Application.Dtos
{
    public class CreateCurrentAccountRequestDto
    {
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public decimal Balance { get; set; }
        public string Pin { get; set; }
        public AccountPrivilege Privilege { get; set; }
        public decimal OverdraftLimit { get; set; }
    }
}
