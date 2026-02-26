namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;

/// <summary>
/// Query to retrieve aggregated summary for pending alerts.
/// </summary>
public sealed record GetPendingAlertsSummaryQuery : ICachedQuery<PendingAlertsSummaryResponse>
{
    public Guid? OwnerId { get; init; }
    public int WindowHours { get; init; } = 24;

    private string? _cacheKey;
    public string GetCacheKey => _cacheKey ?? $"GetPendingAlertsSummaryQuery-{OwnerId}-window-{WindowHours}";

    public TimeSpan? Duration => TimeSpan.FromSeconds(10);
    public TimeSpan? DistributedCacheDuration => TimeSpan.FromSeconds(30);

    public IReadOnlyCollection<string> CacheTags =>
    [
        CacheTagCatalog.Alerts,
        CacheTagCatalog.PendingAlerts
    ];

    public void SetCacheKey(string cacheKey)
    {
        _cacheKey = $"GetPendingAlertsSummaryQuery-{OwnerId}-window-{WindowHours}-{cacheKey}";
    }
}
