using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Application.Models;
using Shared.Domain;
using Shared.Domain.Authorization;
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
        var (statusCode, errorResponse) = exception switch
        {
            AuthenticationRequiredException authReqEx => (
                StatusCodes.Status401Unauthorized,
                CreateErrorResponse(authReqEx.Message, authReqEx.ErrorCode, httpContext)
            ),
            PermissionDeniedException permissionEx => (
                StatusCodes.Status403Forbidden,
                CreateErrorResponse(permissionEx.Message, permissionEx.ErrorCode, httpContext)
            ),
            RequirementFailedException requirementEx => (
                StatusCodes.Status403Forbidden,
                CreateErrorResponse(requirementEx.Message, requirementEx.ErrorCode, httpContext)
            ),
            AuthorizationException authEx => (
                StatusCodes.Status403Forbidden,
                CreateErrorResponse(authEx.Message, authEx.ErrorCode, httpContext)
            ),
            UnauthorizedAccessException unauthorizedEx => (
                StatusCodes.Status403Forbidden,
                CreateErrorResponse(unauthorizedEx.Message, ErrorCodes.FORBIDDEN, httpContext)
            ),
            ValidationException validationEx => HandleValidationException(validationEx, httpContext),
            BusinessRuleViolationException businessEx => (
                StatusCodes.Status400BadRequest,
                CreateErrorResponse(businessEx.Message, businessEx.ErrorCode, httpContext)
            ),
            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                CreateErrorResponse(domainEx.Message, domainEx.ErrorCode, httpContext)
            ),
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                CreateErrorResponse(argEx.Message, ErrorCodes.VALIDATION_ERROR, httpContext)
            ),
            InvalidOperationException invalidEx => (
                StatusCodes.Status400BadRequest,
                CreateErrorResponse(invalidEx.Message, ErrorCodes.INVALID_STATE, httpContext)
            ),
            TimeoutException => (
                StatusCodes.Status408RequestTimeout,
                CreateErrorResponse("Request timeout", ErrorCodes.TIMEOUT, httpContext)
            ),
            OperationCanceledException => (
                StatusCodes.Status408RequestTimeout,
                CreateErrorResponse("Request cancelled", ErrorCodes.TIMEOUT, httpContext)
            ),
            OutOfMemoryException => (
                StatusCodes.Status507InsufficientStorage,
                CreateErrorResponse("System resource exhausted", ErrorCodes.RESOURCE_EXHAUSTED, httpContext)
            ),
            IOException ioEx => (
                StatusCodes.Status507InsufficientStorage,
                CreateErrorResponse(ioEx.Message, ErrorCodes.RESOURCE_EXHAUSTED, httpContext)
            ),
            HttpRequestException => (
                StatusCodes.Status502BadGateway,
                CreateErrorResponse("External service error", ErrorCodes.EXTERNAL_SERVICE_ERROR, httpContext)
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                CreateErrorResponse("Internal server error", ErrorCodes.INTERNAL_ERROR, httpContext)
            )
        };

        LogException(exception, statusCode);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(
            new ApiResponseModel
            {
                Success = false,
                Message = errorResponse.Message,
                Errors = errorResponse.Errors
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }

    private (int, ErrorResponse) HandleValidationException(ValidationException ex, HttpContext context)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return (StatusCodes.Status400BadRequest, new ErrorResponse
        {
            ErrorCode = ErrorCodes.VALIDATION_ERROR,
            Message = "Validation failed",
            Errors = errors,
            TraceId = context.TraceIdentifier
        });
    }

    private ErrorResponse CreateErrorResponse(
        string message,
        string errorCode,
        HttpContext context) => new()
        {
            ErrorCode = errorCode,
            Message = message,
            TraceId = context.TraceIdentifier
        };

    private void LogException(Exception exception, int statusCode)
    {
        var level = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
        _logger.Log(level, exception, "Exception: {Message} | Status: {StatusCode} | Type: {ExceptionType}",
            exception.Message, statusCode, exception.GetType().Name);
    }
}
