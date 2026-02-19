namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetAlertHistory;

/// <summary>
/// Query to retrieve alert history for a specific plot with pagination.
/// Uses PaginatedResponse from SharedKernel for consistency.
/// Implements caching with longer TTL (historical data changes less frequently).
/// </summary>
public sealed record GetAlertHistoryQuery : ICachedQuery<PaginatedResponse<AlertHistoryResponse>>
{
    public Guid PlotId { get; init; }
    public int Days { get; init; } = 30;
    public string? AlertType { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;

    private string? _cacheKey;

    public string GetCacheKey => _cacheKey ??
        $"GetAlertHistoryQuery:plot-{PlotId}:days-{Days}:type-{AlertType ?? "all"}:status-{Status ?? "all"}:page-{PageNumber}:size-{PageSize}";

    public void SetCacheKey(string cacheKey)
    {
        _cacheKey = $"GetAlertHistoryQuery:plot-{PlotId}:days-{Days}:type-{AlertType ?? "all"}:status-{Status ?? "all"}:page-{PageNumber}:size-{PageSize}-{cacheKey}";
    }

    public TimeSpan? Duration => TimeSpan.FromSeconds(30);

    public TimeSpan? DistributedCacheDuration => TimeSpan.FromMinutes(2);

    public IReadOnlyCollection<string> CacheTags => new[]
    {
        CacheTagCatalog.Alerts,
        CacheTagCatalog.AlertHistory,
    };
}
