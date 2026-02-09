namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Response item for pending alert.
/// </summary>
public sealed record PendingAlertResponse(
    Guid Id,
    Guid SensorReadingId,
    string SensorId,
    Guid PlotId,
    string AlertType,
    string Message,
    string Status,
    string Severity,
    double? Value,
    double? Threshold,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt,
    string? AcknowledgedBy);
