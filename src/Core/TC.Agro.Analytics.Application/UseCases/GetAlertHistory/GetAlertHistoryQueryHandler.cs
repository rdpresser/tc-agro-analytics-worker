using Ardalis.Result;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.UseCases.Shared;

namespace TC.Agro.Analytics.Application.UseCases.GetAlertHistory;

/// <summary>
/// Handler for retrieving alert history for a specific plot.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetAlertHistoryQueryHandler 
    : SharedKernel.Application.Handlers.BaseQueryHandler<GetAlertHistoryQuery, AlertListResponse>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetAlertHistoryQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<AlertListResponse>> ExecuteAsync(
        GetAlertHistoryQuery query,
        CancellationToken ct = default)
    {
        var alerts = await _alertReadStore.GetAlertHistoryAsync(
            query.PlotId,
            query.Days,
            query.AlertType,
            query.Status,
            ct);

        var response = GetAlertHistoryMapper.ToListResponse(
            alerts,
            totalCount: alerts.Count,
            pageNumber: 1,
            pageSize: alerts.Count);

        return Result.Success(response);
    }
}
