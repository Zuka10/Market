using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace Market.API.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8]; // Short request ID

        // Log incoming request
        await LogRequestAsync(context, requestId);

        // Capture the original response stream
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // Execute the next middleware in the pipeline
            await _next(context);

            stopwatch.Stop();

            // Log successful response
            await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds, responseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log error response
            _logger.LogError(ex,
                "Request {RequestId} failed after {ElapsedMs}ms - {Method} {Path} -> Exception: {ExceptionType}",
                requestId, stopwatch.ElapsedMilliseconds, context.Request.Method, context.Request.Path, ex.GetType().Name);

            throw; // Re-throw to let global exception handler deal with it
        }
        finally
        {
            // Copy the response back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context, string requestId)
    {
        var request = context.Request;

        // Basic request info
        _logger.LogInformation(
            "Request {RequestId} started - {Method} {Path}{QueryString} from {RemoteIp} | User: {User} | UserAgent: {UserAgent}",
            requestId,
            request.Method,
            request.Path,
            request.QueryString,
            GetClientIpAddress(context),
            context.User?.Identity?.Name ?? "Anonymous",
            request.Headers.UserAgent.ToString()
        );

        // Log request headers (excluding sensitive ones)
        LogRequestHeaders(context, requestId);

        // Log request body for POST/PUT operations
        if (ShouldLogRequestBody(request))
        {
            await LogRequestBodyAsync(context, requestId);
        }
    }

    private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMs, MemoryStream responseBody)
    {
        var response = context.Response;

        _logger.LogInformation(
            "Request {RequestId} completed in {ElapsedMs}ms - {Method} {Path} -> {StatusCode} {StatusDescription} | Size: {ResponseSize}bytes",
            requestId,
            elapsedMs,
            context.Request.Method,
            context.Request.Path,
            response.StatusCode,
            GetStatusDescription(response.StatusCode),
            responseBody.Length
        );

        // Log response headers
        LogResponseHeaders(context, requestId);

        // Log response body for errors or if configured
        if (ShouldLogResponseBody(response))
        {
            await LogResponseBodyAsync(requestId, responseBody);
        }

        // Log performance warning for slow requests
        if (elapsedMs > 5000) // 5 seconds
        {
            _logger.LogWarning(
                "Slow request detected {RequestId} - {Method} {Path} took {ElapsedMs}ms",
                requestId, context.Request.Method, context.Request.Path, elapsedMs
            );
        }
    }

    private void LogRequestHeaders(HttpContext context, string requestId)
    {
        var headers = context.Request.Headers
            .Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        if (headers.Any())
        {
            _logger.LogDebug("Request {RequestId} headers: {Headers}", requestId, JsonSerializer.Serialize(headers));
        }
    }

    private void LogResponseHeaders(HttpContext context, string requestId)
    {
        var headers = context.Response.Headers
            .Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        if (headers.Any())
        {
            _logger.LogDebug("Response {RequestId} headers: {Headers}", requestId, JsonSerializer.Serialize(headers));
        }
    }

    private async Task LogRequestBodyAsync(HttpContext context, string requestId)
    {
        try
        {
            context.Request.EnableBuffering();
            var body = await ReadStreamAsync(context.Request.Body);
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                // Truncate large bodies
                var truncatedBody = body.Length > 2000 ? body[..2000] + "... (truncated)" : body;
                _logger.LogDebug("Request {RequestId} body: {RequestBody}", requestId, truncatedBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log request body for {RequestId}", requestId);
        }
    }

    private async Task LogResponseBodyAsync(string requestId, MemoryStream responseBody)
    {
        try
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var body = await ReadStreamAsync(responseBody);

            if (!string.IsNullOrWhiteSpace(body))
            {
                // Truncate large bodies
                var truncatedBody = body.Length > 2000 ? body[..2000] + "... (truncated)" : body;
                _logger.LogDebug("Response {RequestId} body: {ResponseBody}", requestId, truncatedBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log response body for {RequestId}", requestId);
        }
    }

    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        // Log body for POST, PUT, PATCH operations with JSON content
        return (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH") &&
               request.ContentType?.Contains("application/json") == true &&
               request.ContentLength > 0;
    }

    private static bool ShouldLogResponseBody(HttpResponse response)
    {
        // Log response body for errors (4xx, 5xx) or when response is small JSON
        return (response.StatusCode >= 400) ||
               (response.ContentType?.Contains("application/json") == true && response.Body.Length < 1000);
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "authorization", "cookie", "set-cookie", "x-api-key",
            "x-auth-token", "x-csrf-token", "x-forwarded-for"
        };

        return sensitiveHeaders.Contains(headerName.ToLowerInvariant());
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Try to get real IP from headers (reverse proxy scenarios)
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static string GetStatusDescription(int statusCode)
    {
        return statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            503 => "Service Unavailable",
            _ => "Unknown"
        };
    }
}