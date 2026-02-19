namespace TC.Agro.Analytics.Application.Abstractions.Ports;

/// <summary>
/// Read-only store for querying Alert projections.
/// Following Identity Service pattern: ReadStore returns Response DTOs directly.
/// Uses PaginatedResponse from SharedKernel for consistency.
/// Separates read concerns from write concerns (CQRS pattern).
/// </summary>
public interface IAlertReadStore
{
    /// <summary>
    /// Get all pending alerts with pagination.
    /// Returns PaginatedResponse from SharedKernel (standard pattern).
    /// </summary>
    Task<PaginatedResponse<PendingAlertResponse>> GetPendingAlertsAsync(
        int pageNumber = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get alert history for a specific plot with optional filters and pagination.
    /// Returns PaginatedResponse from SharedKernel (standard pattern).
    /// </summary>
    Task<PaginatedResponse<AlertHistoryResponse>> GetAlertHistoryAsync(
        GetAlertHistoryQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated status metrics for a specific plot.
    /// Returns ready-to-use Response object (no mapping needed in handlers).
    /// </summary>
    Task<GetPlotStatusResponse> GetPlotStatusAsync(
        Guid plotId,
        CancellationToken cancellationToken = default);
}
