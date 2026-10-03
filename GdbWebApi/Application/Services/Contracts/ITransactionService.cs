using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdbWebApi.Application.Dtos;
using GdbWebApi.Domain.Enums;

namespace GdbWebApi.Application.Services.Contracts
{
    public interface ITransactionService
    {
        Task<TResponse> ProcessTransactionAsync<TResponse>(
            TransactionDto transactionDto,
            TransactionType transactionType);
    }
}
