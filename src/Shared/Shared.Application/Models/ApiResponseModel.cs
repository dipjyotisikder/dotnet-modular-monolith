namespace Shared.Application.Models;

public record ApiResponseModel<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }

    public static ApiResponseModel<T> Ok(T data, string? message = null) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponseModel<T> Error(string message, Dictionary<string, string[]>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}

public record ApiResponseModel
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }

    public static ApiResponseModel Ok(string? message = null) =>
        new()
        {
            Success = true,
            Message = message
        };

    public static ApiResponseModel Error(string message, Dictionary<string, string[]>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}
