using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.SharedKernel.Application.Queries;

namespace TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

/// <summary>
/// Query to retrieve aggregated status for a specific plot.
/// </summary>
public sealed record GetPlotStatusQuery : IBaseQuery<PlotStatusResponse>
{
    public Guid PlotId { get; init; }
}
