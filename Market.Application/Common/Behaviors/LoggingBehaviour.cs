using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Market.Application.Common.Behaviors;

public class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting request: {RequestName} at {Timestamp}",
            requestName, DateTime.UtcNow);

        try
        {
            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation("Completed request: {RequestName} in {ElapsedMilliseconds}ms at {Timestamp}",
                requestName, stopwatch.ElapsedMilliseconds, DateTime.UtcNow);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request failed: {RequestName} after {ElapsedMilliseconds}ms at {Timestamp}",
                requestName, stopwatch.ElapsedMilliseconds, DateTime.UtcNow);
            throw;
        }
    }
}