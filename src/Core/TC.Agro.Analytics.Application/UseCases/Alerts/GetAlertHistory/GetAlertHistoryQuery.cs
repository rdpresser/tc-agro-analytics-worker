namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetAlertHistory;

/// <summary>
/// Query to retrieve alert history for a specific sensor with pagination.
/// Uses PaginatedResponse from SharedKernel for consistency.
/// Implements caching with longer TTL (historical data changes less frequently).
/// </summary>
public sealed record GetAlertHistoryQuery : ICachedQuery<PaginatedResponse<AlertHistoryResponse>>
{
    public Guid SensorId { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;
    public int Days { get; init; } = 30;
    public string? AlertType { get; init; }
    public string? Status { get; init; }

    private string? _cacheKey;
    public string GetCacheKey => _cacheKey ??
        $"GetAlertHistoryQuery:sensor-{SensorId}:days-{Days}:type-{AlertType ?? "all"}:status-{Status ?? "all"}:page-{PageNumber}:size-{PageSize}";

    public TimeSpan? Duration => TimeSpan.FromSeconds(30);
    public TimeSpan? DistributedCacheDuration => TimeSpan.FromMinutes(2);

    public IReadOnlyCollection<string> CacheTags =>
    [
        CacheTagCatalog.Alerts,
        CacheTagCatalog.AlertHistory,
    ];

    public void SetCacheKey(string cacheKey)
    {
        _cacheKey = $"GetAlertHistoryQuery:sensor-{SensorId}:days-{Days}:type-{AlertType ?? "all"}:status-{Status ?? "all"}:page-{PageNumber}:size-{PageSize}-{cacheKey}";
    }
}
