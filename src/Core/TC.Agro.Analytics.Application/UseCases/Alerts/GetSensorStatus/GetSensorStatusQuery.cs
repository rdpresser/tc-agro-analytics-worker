namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetSensorStatus;

/// <summary>
/// Query to retrieve aggregated status for a specific sensor.
/// Implements caching with short TTL (real-time dashboard data).
/// </summary>
public sealed record GetSensorStatusQuery : ICachedQuery<GetSensorStatusResponse>
{
    public Guid SensorId { get; init; }

    private string? _cacheKey;
    public string GetCacheKey => _cacheKey ?? $"GetSensorStatusQuery:{SensorId}";

    public TimeSpan? Duration => TimeSpan.FromSeconds(5);
    public TimeSpan? DistributedCacheDuration => TimeSpan.FromSeconds(15);

    public IReadOnlyCollection<string> CacheTags =>
    [
        CacheTagCatalog.Alerts,
        CacheTagCatalog.PendingAlerts,
        CacheTagCatalog.SensorStatus,
    ];

    public void SetCacheKey(string cacheKey)
    {
        _cacheKey = $"GetSensorStatusQuery:{SensorId}-{cacheKey}";
    }
}
