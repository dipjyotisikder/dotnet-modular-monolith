using Shared.Application.Models;
using Shared.Domain;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Mappers;

public static class ResultToHttpResponseMapper
{
    public static IResult MapToHttpResponse<T>(
        Result<T> result,
        Func<T, IResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess != null
                ? onSuccess(result.Value)
                : Results.Ok(new ApiResponseModel<T>
                {
                    Success = true,
                    Data = result.Value
                });
        }

        return MapErrorToHttpResponse(result.Error, result.ErrorCode);
    }

    public static IResult MapToHttpResponse(Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(new ApiResponseModel
            {
                Success = true
            });
        }

        return MapErrorToHttpResponse(result.Error, result.ErrorCode);
    }

    private static IResult MapErrorToHttpResponse(string errorMessage, string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.VALIDATION_ERROR => Results.BadRequest(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.BUSINESS_RULE_VIOLATION => Results.BadRequest(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.RESOURCE_NOT_FOUND => Results.NotFound(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.DUPLICATE_RESOURCE => Results.BadRequest(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.CONFLICT => Results.Conflict(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.FORBIDDEN => Results.Forbid(),
            ErrorCodes.UNAUTHORIZED => Results.Unauthorized(),
            ErrorCodes.EXPIRED => Results.BadRequest(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.INVALID_STATE => Results.BadRequest(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.CONCURRENCY_CONFLICT => Results.Conflict(CreateErrorResponse(errorMessage, errorCode)),
            ErrorCodes.SERVICE_UNAVAILABLE => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
            ErrorCodes.RATE_LIMITED => Results.StatusCode(StatusCodes.Status429TooManyRequests),
            ErrorCodes.DATABASE_ERROR => Results.StatusCode(StatusCodes.Status500InternalServerError),
            ErrorCodes.EXTERNAL_SERVICE_ERROR => Results.StatusCode(StatusCodes.Status502BadGateway),
            ErrorCodes.TIMEOUT => Results.StatusCode(StatusCodes.Status408RequestTimeout),
            ErrorCodes.RESOURCE_EXHAUSTED => Results.StatusCode(StatusCodes.Status507InsufficientStorage),
            _ => Results.BadRequest(CreateErrorResponse(errorMessage, errorCode))
        };
    }

    private static ErrorResponse CreateErrorResponse(
        string message,
        string errorCode,
        Dictionary<string, string[]>? fieldErrors = null)
    {
        return new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Errors = fieldErrors
        };
    }
}
