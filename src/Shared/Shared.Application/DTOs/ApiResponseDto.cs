namespace Shared.Application.DTOs;

public record ApiResponseDto<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }

    public static ApiResponseDto<T> Ok(T data, string? message = null) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponseDto<T> Error(string message, Dictionary<string, string[]>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}

public record ApiResponseDto
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }

    public static ApiResponseDto Ok(string? message = null) =>
        new()
        {
            Success = true,
            Message = message
        };

    public static ApiResponseDto Error(string message, Dictionary<string, string[]>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}
