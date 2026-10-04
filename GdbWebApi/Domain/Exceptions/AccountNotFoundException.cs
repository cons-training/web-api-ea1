using System;

namespace GdbWebApi.Domain.Exceptions
{
    public class AccountNotFoundException : AccountException
    {
        public AccountNotFoundException(
            string message = "Account not found.")
            : base(message)
        {
        }
    }
}