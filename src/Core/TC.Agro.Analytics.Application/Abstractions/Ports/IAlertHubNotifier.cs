namespace TC.Agro.Analytics.Application.Abstractions.Ports;

public interface IAlertHubNotifier
{
    Task NotifyAlertCreatedAsync(
        Guid alertId,
        Guid sensorId,
        string alertType,
        string severity,
        string message,
        double value,
        double threshold,
        DateTimeOffset createdAt);

    Task NotifyAlertAcknowledgedAsync(
        Guid alertId,
        Guid sensorId,
        Guid acknowledgedBy,
        DateTimeOffset acknowledgedAt);

    Task NotifyAlertResolvedAsync(
        Guid alertId,
        Guid sensorId,
        Guid resolvedBy,
        string? resolutionNotes,
        DateTimeOffset resolvedAt);
}
