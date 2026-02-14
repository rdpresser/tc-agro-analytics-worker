namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Query to retrieve all pending alerts with pagination.
/// Uses PaginatedResponse from SharedKernel for consistency.
/// </summary>
public sealed record GetPendingAlertsQuery : IBaseQuery<PaginatedResponse<PendingAlertResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;
}
