using MediatR;
using System.Diagnostics;

namespace DidWeFeedTheCatToday.Features.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogInformation("Starting request {RequestName}", requestName);

            var timer = Stopwatch.StartNew();
            var response = await next();
            timer.Stop();

            logger.LogInformation("Finished request {RequestName} in {Elapsed}ms", requestName, timer.ElapsedMilliseconds);
            return response;
        }
    }
}
