using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.SharedKernel.Application.Queries;

namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Query to retrieve all pending alerts.
/// </summary>
public sealed record GetPendingAlertsQuery : IBaseQuery<AlertListResponse>
{
}
