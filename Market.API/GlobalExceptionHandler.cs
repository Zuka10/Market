using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Market.API;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, detail) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = GetErrorType(exception)
        };

        // Add additional context in development
        if (httpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        // Add correlation ID if available
        if (httpContext.TraceIdentifier != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            // Your existing exceptions from OrderRepository and other repositories
            KeyNotFoundException => (
                StatusCode: (int)HttpStatusCode.NotFound,
                Title: "Resource Not Found",
                Detail: exception.Message
            ),

            ArgumentNullException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Missing Required Field",
                Detail: exception.Message
            ),

            ArgumentException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Invalid Request",
                Detail: exception.Message
            ),

            InvalidOperationException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Invalid Operation",
                Detail: exception.Message
            ),

            UnauthorizedAccessException => (
                StatusCode: (int)HttpStatusCode.Unauthorized,
                Title: "Unauthorized Access",
                Detail: exception.Message
            ),

            // Database/Infrastructure Exceptions
            TimeoutException => (
                StatusCode: (int)HttpStatusCode.RequestTimeout,
                Title: "Request Timeout",
                Detail: "The operation timed out. Please try again."
            ),

            // SQL Server specific exceptions
            Microsoft.Data.SqlClient.SqlException sqlEx => sqlEx.Number switch
            {
                2 => (StatusCode: (int)HttpStatusCode.ServiceUnavailable, Title: "Database Connection Failed", Detail: "Unable to connect to the database."),
                18456 => (StatusCode: (int)HttpStatusCode.Unauthorized, Title: "Database Authentication Failed", Detail: "Database authentication failed."),
                547 => (StatusCode: (int)HttpStatusCode.BadRequest, Title: "Foreign Key Constraint Violation", Detail: "The operation violates a foreign key constraint."),
                2627 => (StatusCode: (int)HttpStatusCode.Conflict, Title: "Duplicate Key Violation", Detail: "A record with this key already exists."),
                _ => (StatusCode: (int)HttpStatusCode.InternalServerError, Title: "Database Error", Detail: "A database error occurred.")
            },

            // Validation Exceptions (if using FluentValidation)
            FluentValidation.ValidationException validationException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Validation Failed",
                Detail: string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))
            ),

            // AutoMapper exceptions
            AutoMapper.AutoMapperMappingException => (
                StatusCode: (int)HttpStatusCode.InternalServerError,
                Title: "Mapping Error",
                Detail: "An error occurred while mapping data."
            ),

            // Task cancellation
            OperationCanceledException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Operation Cancelled",
                Detail: "The operation was cancelled."
            ),

            // Default for unhandled exceptions
            _ => (
                StatusCode: (int)HttpStatusCode.InternalServerError,
                Title: "Internal Server Error",
                Detail: "An unexpected error occurred. Please try again later."
            )
        };
    }

    private static string GetErrorType(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ArgumentNullException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ArgumentException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            InvalidOperationException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            UnauthorizedAccessException => "https://tools.ietf.org/html/rfc7235#section-3.1",
            TimeoutException => "https://tools.ietf.org/html/rfc7231#section-6.5.7",
            Microsoft.Data.SqlClient.SqlException => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            FluentValidation.ValidationException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            AutoMapper.AutoMapperMappingException => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            OperationCanceledException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}