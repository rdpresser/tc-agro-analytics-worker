namespace TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

/// <summary>
/// Query to retrieve aggregated status for a specific plot.
/// </summary>
public sealed record GetPlotStatusQuery : IBaseQuery<GetPlotStatusResponse>
{
    public Guid PlotId { get; init; }
}
