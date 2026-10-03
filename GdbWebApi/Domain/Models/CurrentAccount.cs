using System;
using GdbWebApi.Domain;
using GdbWebApi.Domain.Enums;
using GdbWebApi.Domain.Exceptions;


namespace GdbWebApi.Domain.Models
{
    public class CurrentAccount : Account
    {
        private decimal _overdraftLimit;

        public CurrentAccount(string accountNumber, string name, int age, decimal balance,AccountType accountType, AccountStatus status, string pin,AccountPrivilege privilege, decimal overdraftLimit = 25000.0m)
            : base(accountNumber, name, age, balance, accountType, status, pin, privilege)
        {
            this._overdraftLimit = overdraftLimit;
        }

        public override void ProcessDebit(decimal amount)
        {
            if (!CheckIfOverdraftLimitIsExceeded(amount))
                throw new InsufficientBalanceException("Overdraft limit exceeded");
            _balance -= amount;
            
        }

        public bool CheckIfOverdraftLimitIsExceeded(decimal amount)
        {
            if (amount > (_balance + _overdraftLimit))
            {
                return false;
            }
            return true;
        }

        public decimal OverdraftLimit { get => _overdraftLimit; set => _overdraftLimit = value; }
    }
}
