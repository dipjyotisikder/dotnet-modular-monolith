using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Shared.Domain.Validation;
using System.Text.Json;

namespace Shared.Infrastructure.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            DomainException domainException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Domain Error",
                Detail = domainException.Message,
                Extensions = new Dictionary<string, object?>
                {
                    ["errorCode"] = domainException.ErrorCode
                }
            },
            BusinessRuleViolationException businessRuleException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Business Rule Violation",
                Detail = businessRuleException.Message,
                Extensions = new Dictionary<string, object?>
                {
                    ["errorCode"] = businessRuleException.ErrorCode
                }
            },
            ArgumentException argumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = argumentException.Message,
                Extensions = new Dictionary<string, object?>
                {
                    ["errorCode"] = ErrorCodes.VALIDATION_ERROR
                }
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Extensions = new Dictionary<string, object?>
                {
                    ["errorCode"] = ErrorCodes.INTERNAL_ERROR
                }
            }
        };

        _logger.LogError(exception,
            "Exception occurred: {Message}. Type: {ExceptionType}",
            exception.Message,
            exception.GetType().Name);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }
}
