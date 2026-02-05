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

            if (exception is KeyNotFoundException)
                code = StatusCodes.Status404NotFound;
            else if (exception is ArgumentException)
                code = StatusCodes.Status400BadRequest;


            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;

            var result = JsonSerializer.Serialize(new { error = exception.Message });

            return context.Response.WriteAsync(result);
        }
    }
}
