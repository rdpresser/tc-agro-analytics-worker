namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Query to retrieve all pending alerts with pagination.
/// Uses PaginatedResponse from SharedKernel for consistency.
/// Implements caching to reduce database load for frequently accessed data.
/// </summary>
public sealed record GetPendingAlertsQuery : ICachedQuery<PaginatedResponse<PendingAlertResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;

    private string? _cacheKey;
    public string GetCacheKey => _cacheKey ?? $"GetPendingAlertsQuery-{PageNumber}:size-{PageSize}";

    public void SetCacheKey(string cacheKey)
    {
        _cacheKey = $"{cacheKey}:GetPendingAlertsQuery:page-{PageNumber}:size-{PageSize}";
    }

    public TimeSpan? Duration => TimeSpan.FromSeconds(10);

    public TimeSpan? DistributedCacheDuration => TimeSpan.FromSeconds(30);

    public IReadOnlyCollection<string> CacheTags => new[] { "alerts", "GetPendingAlertsQuery" };
}
