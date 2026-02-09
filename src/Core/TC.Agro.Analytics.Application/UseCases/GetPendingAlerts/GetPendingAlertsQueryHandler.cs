namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Handler for retrieving all pending alerts.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetPendingAlertsQueryHandler 
    : SharedKernel.Application.Handlers.BaseHandler<GetPendingAlertsQuery, AlertListResponse>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetPendingAlertsQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<AlertListResponse>> ExecuteAsync(
        GetPendingAlertsQuery query,
        CancellationToken ct = default)
    {
        var allAlerts = await _alertReadStore.GetPendingAlertsAsync(ct);

        // Apply pagination
        var totalCount = allAlerts.Count;
        var paginatedAlerts = allAlerts
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var response = GetPendingAlertsMapper.ToListResponse(
            paginatedAlerts,
            totalCount: totalCount,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize);

        return Result.Success(response);
    }
}
