namespace TC.Agro.Analytics.Application.UseCases.Shared;

/// <summary>
/// Pagination parameters for queries.
/// </summary>
public sealed record PaginationParams
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 100;
    
    public int Skip => (PageNumber - 1) * PageSize;
    
    public const int MaxPageSize = 500;
    public const int DefaultPageSize = 100;
}

/// <summary>
/// Paginated response wrapper.
/// </summary>
public sealed record PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
