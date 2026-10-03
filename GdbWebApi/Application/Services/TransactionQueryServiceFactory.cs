using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdbWebApi.Application.Services
{
    public static class TransactionQueryServiceFactory
    {
        public static ITransactionQueryService Create()
        {
            return new TransactionQueryService();
        }
    }
}
