namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;

/// <summary>
/// Response item for pending alert.
/// </summary>
public sealed record PendingAlertResponse(
    Guid Id,
    Guid SensorId,
    string AlertType,
    string Message,
    string Status,
    string Severity,
    double? Value,
    double? Threshold,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AcknowledgedAt,
    Guid? AcknowledgedBy,
    DateTimeOffset? ResolvedAt = null,
    Guid? ResolvedBy = null,
    string? ResolutionNotes = null,
    string? PlotName = null,
    string? PropertyName = null);
