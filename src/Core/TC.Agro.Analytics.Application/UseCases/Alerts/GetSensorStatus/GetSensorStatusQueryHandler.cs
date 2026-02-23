namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetSensorStatus;

/// <summary>
/// Handler for retrieving aggregated sensor status.
/// Following Identity Service pattern: ReadStore returns Response directly.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetSensorStatusQueryHandler 
    : BaseHandler<GetSensorStatusQuery, GetSensorStatusResponse>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetSensorStatusQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<GetSensorStatusResponse>> ExecuteAsync(
        GetSensorStatusQuery query,
        CancellationToken ct = default)
    {
        var response = await _alertReadStore.GetSensorStatusAsync(query.SensorId, ct);
        return Result.Success(response);
    }
}
