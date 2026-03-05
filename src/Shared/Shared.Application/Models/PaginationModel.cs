namespace Shared.Application.Models;

public record PaginationModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
}

public record PaginatedResponseModel<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public PaginationModel Pagination { get; init; } = new();
}
