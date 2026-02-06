using Ardalis.Result;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.UseCases.Shared;

namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Handler for retrieving all pending alerts.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetPendingAlertsQueryHandler 
    : SharedKernel.Application.Handlers.BaseQueryHandler<GetPendingAlertsQuery, AlertListResponse>
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
        var alerts = await _alertReadStore.GetPendingAlertsAsync(ct);

        var response = GetPendingAlertsMapper.ToListResponse(
            alerts,
            totalCount: alerts.Count,
            pageNumber: 1,
            pageSize: alerts.Count);

        return Result.Success(response);
    }
}
