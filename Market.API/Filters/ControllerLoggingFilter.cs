using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Market.API.Filters;

public class ControllerLoggingFilter(ILogger<ControllerLoggingFilter> logger) : IAsyncActionFilter
{
    private readonly ILogger<ControllerLoggingFilter> _logger = logger;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var actionId = Guid.NewGuid().ToString("N")[..8];

        // Extract action information
        var controllerName = context.ActionDescriptor.RouteValues["controller"];
        var actionName = context.ActionDescriptor.RouteValues["action"];
        var httpMethod = context.HttpContext.Request.Method;
        var route = context.HttpContext.Request.Path;
        var userId = context.HttpContext.User?.Identity?.Name ?? "Anonymous";

        // Log action start
        _logger.LogInformation(
            "Action {ActionId} started - {ControllerName}.{ActionName} | {HttpMethod} {Route} | User: {UserId}",
            actionId, controllerName, actionName, httpMethod, route, userId
        );

        // Log action parameters
        LogActionParameters(context, actionId);

        // Log model state if validation failed
        if (!context.ModelState.IsValid)
        {
            LogModelStateErrors(context, actionId);
        }

        try
        {
            // Execute the action
            var executedContext = await next();
            stopwatch.Stop();

            // Log successful completion
            LogActionCompletion(executedContext, actionId, controllerName, actionName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log action failure
            _logger.LogError(ex,
                "Action {ActionId} failed after {ElapsedMs}ms - {ControllerName}.{ActionName} | Exception: {ExceptionType}: {ExceptionMessage}",
                actionId, stopwatch.ElapsedMilliseconds, controllerName, actionName, ex.GetType().Name, ex.Message
            );

            throw; // Re-throw to let global exception handler deal with it
        }
    }

    private void LogActionParameters(ActionExecutingContext context, string actionId)
    {
        if (context.ActionArguments.Any())
        {
            var parameters = context.ActionArguments
                .Where(param => !IsSensitiveParameter(param.Key))
                .ToDictionary(
                    param => param.Key,
                    param => SanitizeParameterValue(param.Value)
                );

            if (parameters.Any())
            {
                _logger.LogDebug(
                    "Action {ActionId} parameters: {Parameters}",
                    actionId,
                    JsonSerializer.Serialize(parameters, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    })
                );
            }
        }
    }

    private void LogModelStateErrors(ActionExecutingContext context, string actionId)
    {
        var errors = context.ModelState
            .Where(ms => ms.Value?.Errors.Count > 0)
            .ToDictionary(
                ms => ms.Key,
                ms => ms.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        _logger.LogWarning(
            "Action {ActionId} has model validation errors: {ValidationErrors}",
            actionId,
            JsonSerializer.Serialize(errors)
        );
    }

    private void LogActionCompletion(ActionExecutedContext context, string actionId, string? controllerName, string? actionName, long elapsedMs)
    {
        var httpStatusCode = context.HttpContext.Response.StatusCode;
        var resultType = context.Result?.GetType().Name ?? "Unknown";

        _logger.LogInformation(
            "Action {ActionId} completed in {ElapsedMs}ms - {ControllerName}.{ActionName} | Status: {StatusCode} | Result: {ResultType}",
            actionId, elapsedMs, controllerName, actionName, httpStatusCode, resultType
        );

        // Log result details for specific result types
        LogActionResult(context, actionId);

        // Log performance warning for slow actions
        if (elapsedMs > 3000) // 3 seconds
        {
            _logger.LogWarning(
                "Slow action detected {ActionId} - {ControllerName}.{ActionName} took {ElapsedMs}ms",
                actionId, controllerName, actionName, elapsedMs
            );
        }
    }

    private void LogActionResult(ActionExecutedContext context, string actionId)
    {
        if (context.Result == null)
        {
            return;
        }

        try
        {
            switch (context.Result)
            {
                case BadRequestObjectResult badRequestResult:
                    _logger.LogDebug(
                        "Action {ActionId} returned BadRequest: {ErrorDetails}",
                        actionId, JsonSerializer.Serialize(badRequestResult.Value)
                    );
                    break;

                case NotFoundObjectResult notFoundResult:
                    _logger.LogDebug(
                        "Action {ActionId} returned NotFound: {ErrorDetails}",
                        actionId, JsonSerializer.Serialize(notFoundResult.Value)
                    );
                    break;

                case ObjectResult objectResult:
                    var value = SanitizeResultValue(objectResult.Value);
                    _logger.LogDebug(
                        "Action {ActionId} returned ObjectResult with status {StatusCode}: {ResultValue}",
                        actionId, objectResult.StatusCode, JsonSerializer.Serialize(value)
                    );
                    break;

                case JsonResult jsonResult:
                    var jsonValue = SanitizeResultValue(jsonResult.Value);
                    _logger.LogDebug(
                        "Action {ActionId} returned JsonResult: {ResultValue}",
                        actionId, JsonSerializer.Serialize(jsonValue)
                    );
                    break;

                case StatusCodeResult statusCodeResult:
                    _logger.LogDebug(
                        "Action {ActionId} returned StatusCode: {StatusCode}",
                        actionId, statusCodeResult.StatusCode
                    );
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log action result for {ActionId}", actionId);
        }
    }

    private static bool IsSensitiveParameter(string parameterName)
    {
        var sensitiveParams = new[]
        {
            "password", "token", "secret", "key", "authorization",
            "creditcard", "ssn", "api_key", "auth_token"
        };

        return sensitiveParams.Any(sensitive =>
            parameterName.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }

    private static object? SanitizeParameterValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        // For complex objects, we might want to remove sensitive properties
        var json = JsonSerializer.Serialize(value);

        // Truncate large parameter values
        if (json.Length > 1000)
        {
            return json[..1000] + "... (truncated)";
        }

        return value;
    }

    private static object? SanitizeResultValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        var json = JsonSerializer.Serialize(value);

        // Truncate large result values
        if (json.Length > 2000)
        {
            return json[..2000] + "... (truncated)";
        }

        return value;
    }
}