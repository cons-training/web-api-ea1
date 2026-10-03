using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Domain.Enums;
using GdbWebApi.Infrastructure.Repositories.Contracts;
using GdbWebApi.Infrastructure.Repositories.Implementations;
using gdb.Logging;
using Microsoft.Extensions.Logging;

namespace GdbWebApi.Application.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private static readonly ILogger _logger =
            AppLogger.CreateLogger<TransactionService>();

        public TransactionService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }

        public TransactionService()
            : this(new AccountRepositoryDB(), new TransactionRepositoryDB())
        {
        }

        public async Task<TResponse> ProcessTransactionAsync<TResponse>(
            TransactionDto transactionDto,
            TransactionType transactionType)
        {
            _logger.LogInformation(
                "Processing transaction {TransactionType}",
                transactionType);

            ITransactionCommand<TResponse> command = transactionType switch
            {
                TransactionType.Deposit =>
                    (ITransactionCommand<TResponse>)new DepositTransactionCommand(
                        _accountRepository,
                        _transactionRepository),

                TransactionType.Withdraw =>
                    (ITransactionCommand<TResponse>)new WithdrawTransactionCommand(
                        _accountRepository,
                        _transactionRepository),

                TransactionType.Transfer =>
                    (ITransactionCommand<TResponse>)new TransferTransactionCommand(
                        _accountRepository,
                        _transactionRepository),

                _ => throw new ArgumentException(
                    $"Invalid transaction type: {transactionType}")
            };

            TResponse response =
                await command.ExecuteAsync(transactionDto);

            _logger.LogInformation(
                "Transaction {TransactionType} completed successfully",
                transactionType);

            return response;
        }
    }
}
