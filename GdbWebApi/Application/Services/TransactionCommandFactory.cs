using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using GdbWebApi.Domain.Enums;
using GdbWebApi.Infrastructure.Repositories;
using GdbWebApi.Infrastructure.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdbWebApi.Application.Services
{
    public static class TransactionCommandFactory
    {
        public static ITransactionCommand<TResponse> Create<TResponse>(
            TransactionType transactionType)
        {
            IAccountRepository accountRepository =
                AccountRepositoryFactory.Create("DB");

            ITransactionRepository transactionRepository =
                TransactionRepositoryFactory.Create("DB");

            return transactionType switch
            {
                TransactionType.Deposit =>
                    (ITransactionCommand<TResponse>)new DepositTransactionCommand(
                        accountRepository,
                        transactionRepository),

                TransactionType.Withdraw =>
                    (ITransactionCommand<TResponse>)new WithdrawTransactionCommand(
                        accountRepository,
                        transactionRepository),

                TransactionType.Transfer =>
                    (ITransactionCommand<TResponse>)new TransferTransactionCommand(
                        accountRepository,
                        transactionRepository),

                _ => throw new ArgumentException(
                    $"Invalid transaction type: {transactionType}")
            };
        }
    }
}
