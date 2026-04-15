using Microsoft.AspNetCore.Diagnostics;
using Products.Api.Contracts.Common;
using System.Net;
using System.Text.Json;

namespace Products.Api.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.FailureResponse(exception.Message);

            context.Response.StatusCode = exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json, cancellationToken);

            return true; // handled
        }
    }
}
