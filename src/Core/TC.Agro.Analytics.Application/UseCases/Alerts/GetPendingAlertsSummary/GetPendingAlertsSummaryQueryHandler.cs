namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;

internal sealed class GetPendingAlertsSummaryQueryHandler
    : BaseHandler<GetPendingAlertsSummaryQuery, PendingAlertsSummaryResponse>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetPendingAlertsSummaryQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<PendingAlertsSummaryResponse>> ExecuteAsync(
        GetPendingAlertsSummaryQuery query,
        CancellationToken ct = default)
    {
        var response = await _alertReadStore.GetPendingAlertsSummaryAsync(
            query.OwnerId,
            query.WindowHours,
            ct);

        return Result.Success(response);
    }
}
