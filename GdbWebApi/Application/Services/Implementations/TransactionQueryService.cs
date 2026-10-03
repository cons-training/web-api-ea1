using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Domain.Exceptions;
using GdbWebApi.Domain.Models;
using GdbWebApi.Infrastructure.Repositories.Contracts;
using GdbWebApi.Infrastructure.Repositories.Implementations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GdbWebApi.Application.Services.Implementations
{
    public class TransactionQueryService : ITransactionQueryService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public TransactionQueryService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }

        public TransactionQueryService()
            : this(new AccountRepositoryDB(), new TransactionRepositoryDB())
        {
        }

        public async Task<List<ViewRecentTransactionsResponseDto>>
            GetRecentTransactionsAsync(string accountNumber)
        {
            IAccount account =
                await _accountRepository.GetAccountAsync(accountNumber);

            if (account == null)
            {
                throw new AccountException("Account not found.");
            }

            return _transactionRepository.GetRecentTransactions(accountNumber);
        }
    }
}
