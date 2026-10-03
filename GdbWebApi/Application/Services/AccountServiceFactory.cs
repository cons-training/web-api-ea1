using GdbWebApi.Application.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using GdbWebApi.Application.Services.Contracts;

namespace GdbWebApi.Application.Services
{
    public class AccountServiceFactory
    {
        public static IAccountService Create()
        {
            return new AccountService();
        }
    }
}