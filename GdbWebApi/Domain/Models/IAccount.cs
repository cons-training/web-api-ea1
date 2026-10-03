using System;
using GdbWebApi.Domain;
using GdbWebApi.Domain.Enums;
using GdbWebApi.Domain.Exceptions;
namespace GdbWebApi.Domain.Models

{
    public interface IAccount
    {
        string AccountNumber { get; }
        string Name { get; }
        int Age { get; }
        decimal Balance { get; }
        AccountType AccountType { get; }
        AccountStatus Status { get; }

        AccountPrivilege Privilege { get; }


        bool CheckIfAccountIsActive(); 
        void Deposit(decimal amount);
        void Withdraw(decimal amount, string enteredPin);
        bool ValidatePin(string enteredPin);

        bool ChangePin(string oldPIn, string newPin);

    }
}