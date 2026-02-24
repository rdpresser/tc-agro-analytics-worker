namespace TC.Agro.Analytics.Service.Hubs;

public sealed record AlertCreatedNotification(
    Guid AlertId,
    Guid SensorId,
    string? SensorLabel,
    Guid PlotId,
    string PlotName,
    string PropertyName,
    string Type,
    string Severity,
    string Message,
    double Value,
    double Threshold,
    string? Metadata,
    DateTimeOffset CreatedAt);
