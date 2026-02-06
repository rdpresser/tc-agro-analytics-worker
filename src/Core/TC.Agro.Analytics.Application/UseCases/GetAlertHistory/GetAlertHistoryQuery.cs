using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.SharedKernel.Application.Queries;

namespace TC.Agro.Analytics.Application.UseCases.GetAlertHistory;

/// <summary>
/// Query to retrieve alert history for a specific plot.
/// </summary>
public sealed record GetAlertHistoryQuery : IBaseQuery<AlertListResponse>
{
    public Guid PlotId { get; init; }
    public int Days { get; init; } = 30;
    public string? AlertType { get; init; }
    public string? Status { get; init; }
}
