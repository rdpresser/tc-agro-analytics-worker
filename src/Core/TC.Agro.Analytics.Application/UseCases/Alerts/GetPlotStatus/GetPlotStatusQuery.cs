namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPlotStatus;

/// <summary>
/// Query to retrieve aggregated status for a specific plot.
/// Implements caching with short TTL (real-time dashboard data).
/// </summary>
public sealed record GetPlotStatusQuery : ICachedQuery<GetPlotStatusResponse>
{
    public Guid PlotId { get; init; }

    private string? _cacheKey;

    public string GetCacheKey => _cacheKey ?? $"GetPlotStatusQuery:{PlotId}";

    public void SetCacheKey(string cacheKey)
    {
        _cacheKey = $"GetPlotStatusQuery:{PlotId}-{cacheKey}";
    }

    public TimeSpan? Duration => TimeSpan.FromSeconds(5);

    public TimeSpan? DistributedCacheDuration => TimeSpan.FromSeconds(15);

    public IReadOnlyCollection<string> CacheTags => new[]
    {
        CacheTagCatalog.Alerts,
        CacheTagCatalog.PendingAlerts,
        CacheTagCatalog.PlotStatus,
    };
}
