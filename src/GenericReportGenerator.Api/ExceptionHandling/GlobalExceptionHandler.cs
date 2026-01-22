using FluentValidation;
using GenericReportGenerator.Infrastructure.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace GenericReportGenerator.Api.ExceptionHandling;

/// <summary>
/// Handles exceptions globally for HTTP requests.
/// </summary>
/// <remarks>
/// This handler translates validation, domain and system exceptions into structured error responses with
/// relevant HTTP status codes, and logs them all. It is intended to be used as a centralized exception handler in this API.
/// </remarks>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        object response;
        int statusCode;

        switch (exception)
        {
            // Validation exceptions. Http code - 400.
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;

                // Group validation errors by field.
                Dictionary<string, string[]> errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                response = new ValidationErrorResponse(
                    Name: nameof(ValidationException),
                    TraceId: traceId,
                    Errors: errors
                );

                _logger.LogInformation("Validation failed.");
                break;

            // Domain exceptions. Http code - 422.
            case DomainException domainException:
                statusCode = StatusCodes.Status422UnprocessableEntity;

                response = new DetailErrorResponse(
                    Name: domainException.GetType().Name,
                    TraceId: traceId,
                    Details: domainException.Message
                );

                _logger.LogWarning(domainException, "Domain error: {ErrorDetails}", domainException.Message);
                break;

            // System exceptions. Http code - 500.
            default:
                statusCode = StatusCodes.Status500InternalServerError;

                response = new DetailErrorResponse(
                    Name: "SystemException",
                    TraceId: traceId,
                    Details: "Internal server error." 
                );

                _logger.LogError(exception, "System error: {ErrorDetails}", exception.Message);
                break;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response, ct);

        return true;
    }
}