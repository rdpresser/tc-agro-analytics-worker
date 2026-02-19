namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetAlertHistory;

/// <summary>
/// Response item for alert in history list.
/// </summary>
public sealed record AlertHistoryResponse(
    Guid Id,
    Guid SensorReadingId,
    Guid SensorId,
    Guid PlotId,
    string AlertType,
    string Message,
    string Status,
    string Severity,
    double? Value,
    double? Threshold,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt,
    string? AcknowledgedBy,
    DateTime? ResolvedAt,
    string? ResolvedBy,
    string? ResolutionNotes);
