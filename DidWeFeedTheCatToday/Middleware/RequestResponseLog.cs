namespace DidWeFeedTheCatToday.Middleware
{
    public class RequestResponseLog(RequestDelegate next, ILogger<RequestResponseLog> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestResponseLog> _logger = logger;


        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var timeOfRequest = DateTime.UtcNow;

            _logger.LogInformation("Handling request: {0} {1}", request.Method, request.Path);

            await _next(context);

            var timeElapsed = (DateTime.UtcNow - timeOfRequest).TotalMilliseconds;

            _logger.LogInformation("Response: {0}. {1} ms.", context.Response.StatusCode, timeElapsed);
        }
    }
}
