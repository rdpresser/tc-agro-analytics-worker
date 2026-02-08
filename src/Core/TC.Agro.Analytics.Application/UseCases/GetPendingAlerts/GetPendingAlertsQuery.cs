using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.SharedKernel.Application.Queries;

namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Query to retrieve all pending alerts with pagination.
/// </summary>
public sealed record GetPendingAlertsQuery : IBaseQuery<AlertListResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;
}
