using TC.Agro.Analytics.Domain.Entities;

namespace TC.Agro.Analytics.Application.Abstractions.Ports;

/// <summary>
/// Read-only store for querying Alert projections.
/// Separates read concerns from write concerns (CQRS pattern).
/// </summary>
public interface IAlertReadStore
{
    /// <summary>
    /// Get all pending alerts (last 100, ordered by creation date descending)
    /// </summary>
    Task<List<Alert>> GetPendingAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get alert history for a specific plot with optional filters
    /// </summary>
    Task<List<Alert>> GetAlertHistoryAsync(
        Guid plotId,
        int days = 30,
        string? alertType = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated status metrics for a specific plot
    /// </summary>
    Task<PlotStatusResult> GetPlotStatusAsync(
        Guid plotId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result object for plot status query (internal use, not exposed to API)
/// </summary>
public record PlotStatusResult
{
    public Guid PlotId { get; init; }
    public int PendingAlertsCount { get; init; }
    public int TotalAlertsLast24Hours { get; init; }
    public int TotalAlertsLast7Days { get; init; }
    public Alert? MostRecentAlert { get; init; }
    public Dictionary<string, int> AlertsByType { get; init; } = new();
    public Dictionary<string, int> AlertsBySeverity { get; init; } = new();
    public string OverallStatus { get; init; } = "OK";
}
