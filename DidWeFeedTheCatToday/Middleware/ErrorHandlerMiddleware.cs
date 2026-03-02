using DidWeFeedTheCatToday.Shared.Common;
using System.Text.Json;

namespace DidWeFeedTheCatToday.Middleware
{
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
            var code = StatusCodes.Status500InternalServerError;
            var message = "A server side error occured";

            if (exception is KeyNotFoundException)
            {
                code = StatusCodes.Status404NotFound;
                message = exception.Message;
            }
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;

            var result = JsonSerializer.Serialize(ApiResponse<string>.Fail(message));

            return context.Response.WriteAsync(result);
        }
    }
}
