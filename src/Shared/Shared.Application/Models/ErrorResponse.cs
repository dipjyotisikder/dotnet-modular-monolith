namespace Shared.Application.Models;

public record ErrorResponse
{
    public string ErrorCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; init; }
    public string? TraceId { get; init; }
}
