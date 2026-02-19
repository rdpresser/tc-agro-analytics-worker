namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPlotStatus;

/// <summary>
/// Alert information included in plot status.
/// </summary>
public sealed record PlotStatusAlertResponse(
    Guid Id,
    Guid SensorReadingId,
    Guid SensorId,
    string AlertType,
    string Message,
    string Status,
    string Severity,
    double? Value,
    double? Threshold,
    DateTimeOffset CreatedAt);

/// <summary>
/// Plot status summary response with aggregated alert information.
/// </summary>
public sealed record GetPlotStatusResponse(
    Guid PlotId,
    int PendingAlertsCount,
    int TotalAlertsLast24Hours,
    int TotalAlertsLast7Days,
    PlotStatusAlertResponse? MostRecentAlert,
    Dictionary<string, int> AlertsByType,
    Dictionary<string, int> AlertsBySeverity,
    string OverallStatus);
