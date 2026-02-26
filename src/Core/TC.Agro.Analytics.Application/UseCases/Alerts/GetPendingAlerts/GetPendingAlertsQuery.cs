namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;

/// <summary>
/// Query to retrieve all pending alerts with pagination.
/// Uses PaginatedResponse from SharedKernel for consistency.
/// Implements caching to reduce database load for frequently accessed data.
/// </summary>
public sealed record GetPendingAlertsQuery : ICachedQuery<PaginatedResponse<PendingAlertResponse>>
{
    public Guid? OwnerId { get; init; }
    public string? Search { get; init; }
    public string? Severity { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;

    private string? _cacheKey;
    public string GetCacheKey => _cacheKey
        ?? $"GetPendingAlertsQuery-{OwnerId}-page-{PageNumber}:size-{PageSize}:search-{Search}:severity-{Severity}:status-{Status}";

    public TimeSpan? Duration => TimeSpan.FromSeconds(10);
    public TimeSpan? DistributedCacheDuration => TimeSpan.FromSeconds(30);

    public IReadOnlyCollection<string> CacheTags =>
    [
        CacheTagCatalog.Alerts,
        CacheTagCatalog.PendingAlerts
    ];

    public void SetCacheKey(string cacheKey)
    {
        _cacheKey = $"GetPendingAlertsQuery-{OwnerId}-page-{PageNumber}:size-{PageSize}:search-{Search}:severity-{Severity}:status-{Status}-{cacheKey}";
    }
}
