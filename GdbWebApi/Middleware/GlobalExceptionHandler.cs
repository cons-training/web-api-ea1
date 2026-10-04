using GdbWebApi.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace GdbWebApi.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var (statusCode, message) = exception switch
            {
                AccountNotFoundException =>
                    ((int)HttpStatusCode.NotFound, exception.Message),

                InvalidPinException =>
                    ((int)HttpStatusCode.Unauthorized, exception.Message),

                InsufficientBalanceException or
                InvalidAmountException or
                MinimumBalanceViolationException or
                InactiveAccountException or
                AccountException or
                InvalidOperationException =>
                    ((int)HttpStatusCode.BadRequest, exception.Message),

                _ =>
                    ((int)HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.")
            };

            httpContext.Response.StatusCode = statusCode;

            var response = new
            {
                statusCode,
                message
            };

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }
    }
}