using DidWeFeedTheCatToday.Shared.Common;
using FluentValidation;
using System.Text.Json;

namespace DidWeFeedTheCatToday.Middleware
{
    /// <summary>
    /// Global interceptor for unhandled exceptions.
    /// Standardizes error responses into a consistent ApiResponse format for all clients.
    /// </summary>
    public class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger = logger;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception.");
                await HandleErrorAsync(context, e);
            }
        }

        private static Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            var (code, message) = exception switch
            {
                ValidationException vEx =>
                    (StatusCodes.Status400BadRequest, string.Join(" | ", vEx.Errors.Select(e => e.ErrorMessage))),

                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound, exception.Message),

                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;

            var result = JsonSerializer.Serialize(ApiResponse<string>.Fail(message));

            return context.Response.WriteAsync(result);
        }
    }
}
