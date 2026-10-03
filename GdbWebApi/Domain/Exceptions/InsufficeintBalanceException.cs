using System;
using GdbWebApi.Domain.Exceptions;

namespace GdbWebApi.Domain.Exceptions
{
    /// <summary>
    /// Purpose: Thrown when withdrawal amount exceeds available funds.
    /// </summary>
    public class InsufficientBalanceException : AccountException
    {
        public InsufficientBalanceException(string message = "") : base(message) { }
    }
}
