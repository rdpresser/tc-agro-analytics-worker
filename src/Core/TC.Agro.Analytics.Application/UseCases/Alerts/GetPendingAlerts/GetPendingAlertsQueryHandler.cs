namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;

/// <summary>
/// Handler for retrieving all pending alerts.
/// Following Identity Service pattern: ReadStore returns PaginatedResponse from SharedKernel.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetPendingAlertsQueryHandler
    : BaseHandler<GetPendingAlertsQuery, PaginatedResponse<PendingAlertResponse>>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetPendingAlertsQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<PaginatedResponse<PendingAlertResponse>>> ExecuteAsync(
        GetPendingAlertsQuery query,
        CancellationToken ct = default)
    {
        var response = await _alertReadStore.GetPendingAlertsAsync(
            query.OwnerId,
            query.Search,
            query.Severity,
            query.Status,
            query.PageNumber,
            query.PageSize,
            ct);

        return Result.Success(response);
    }
}
