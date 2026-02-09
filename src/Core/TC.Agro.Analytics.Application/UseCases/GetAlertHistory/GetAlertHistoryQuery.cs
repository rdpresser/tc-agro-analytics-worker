namespace TC.Agro.Analytics.Application.UseCases.GetAlertHistory;

/// <summary>
/// Query to retrieve alert history for a specific plot with pagination.
/// Uses PaginatedResponse from SharedKernel for consistency.
/// </summary>
public sealed record GetAlertHistoryQuery : IBaseQuery<PaginatedResponse<AlertHistoryResponse>>
{
    public Guid PlotId { get; init; }
    public int Days { get; init; } = 30;
    public string? AlertType { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;
}
