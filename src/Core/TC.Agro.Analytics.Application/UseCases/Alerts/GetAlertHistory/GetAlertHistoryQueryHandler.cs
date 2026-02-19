namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetAlertHistory;

/// <summary>
/// Handler for retrieving alert history for a specific plot.
/// Following Identity Service pattern: ReadStore returns PaginatedResponse from SharedKernel.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetAlertHistoryQueryHandler 
    : BaseHandler<GetAlertHistoryQuery, PaginatedResponse<AlertHistoryResponse>>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetAlertHistoryQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<PaginatedResponse<AlertHistoryResponse>>> ExecuteAsync(
        GetAlertHistoryQuery query,
        CancellationToken ct = default)
    {
        var response = await _alertReadStore.GetAlertHistoryAsync(query, ct).ConfigureAwait(false);

        return Result.Success(response);
    }
}
