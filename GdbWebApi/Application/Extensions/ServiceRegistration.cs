using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using GdbWebApi.Infrastructure.Repositories.Contracts;
using GdbWebApi.Infrastructure.Repositories.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace GdbWebApi.Application.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddBankingServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAccountRepository, AccountRepositoryDB>();
            services.AddScoped<ITransactionRepository, TransactionRepositoryDB>();
            services.AddScoped<IHomeRepository, HomeRepository>();

            // Application Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransactionQueryService, TransactionQueryService>();
            services.AddScoped<IHomeService, HomeService>();

            return services;
        }
    }
}
