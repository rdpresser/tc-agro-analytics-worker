using Ardalis.Result;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.UseCases.Shared;

namespace TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

/// <summary>
/// Handler for retrieving aggregated plot status.
/// Uses IAlertReadStore for read-only queries (CQRS pattern).
/// </summary>
internal sealed class GetPlotStatusQueryHandler 
    : SharedKernel.Application.Handlers.BaseQueryHandler<GetPlotStatusQuery, PlotStatusResponse>
{
    private readonly IAlertReadStore _alertReadStore;

    public GetPlotStatusQueryHandler(IAlertReadStore alertReadStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
    }

    public override async Task<Result<PlotStatusResponse>> ExecuteAsync(
        GetPlotStatusQuery query,
        CancellationToken ct = default)
    {
        var plotStatus = await _alertReadStore.GetPlotStatusAsync(query.PlotId, ct);
        var response = GetPlotStatusMapper.ToResponse(plotStatus);
        return Result.Success(response);
    }
}
