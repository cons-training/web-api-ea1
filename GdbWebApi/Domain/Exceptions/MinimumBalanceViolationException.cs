using System;
using GdbWebApi.Domain.Exceptions;


namespace GdbWebApi.Domain.Exceptions
{
    /// <summary>
    /// Purpose: Thrown when withdrawal breaches minimum balance requirement.
    /// </summary>
    public class MinimumBalanceViolationException : AccountException
    {
        public MinimumBalanceViolationException(string message = "") : base(message) { }
    }
}
