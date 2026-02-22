namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetSensorStatus;

/// <summary>
/// Alert information included in sensor status.
/// </summary>
public sealed record SensorStatusAlertResponse(
    Guid Id,
    Guid SensorId,
    string AlertType,
    string Message,
    string Status,
    string Severity,
    double? Value,
    double? Threshold,
    DateTimeOffset CreatedAt);

/// <summary>
/// Sensor status summary response with aggregated alert information.
/// </summary>
public sealed record GetSensorStatusResponse(
    Guid SensorId,
    int PendingAlertsCount,
    int TotalAlertsLast24Hours,
    int TotalAlertsLast7Days,
    SensorStatusAlertResponse? MostRecentAlert,
    Dictionary<string, int> AlertsByType,
    Dictionary<string, int> AlertsBySeverity,
    string OverallStatus);
