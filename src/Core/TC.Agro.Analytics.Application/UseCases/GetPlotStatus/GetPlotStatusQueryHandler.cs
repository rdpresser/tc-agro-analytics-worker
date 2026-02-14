namespace TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

/// <summary>
/// Handler for retrieving aggregated plot status.
/// Following Identity Service pattern: ReadStore returns Response directly.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetPlotStatusQueryHandler 
    : BaseHandler<GetPlotStatusQuery, GetPlotStatusResponse>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetPlotStatusQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<GetPlotStatusResponse>> ExecuteAsync(
        GetPlotStatusQuery query,
        CancellationToken ct = default)
    {
        var response = await _alertReadStore.GetPlotStatusAsync(query.PlotId, ct);
        return Result.Success(response);
    }
}
