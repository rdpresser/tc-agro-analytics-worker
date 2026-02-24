namespace TC.Agro.Analytics.Service.Hubs;

public sealed record AlertAcknowledgedNotification(
    Guid AlertId,
    Guid SensorId,
    string SensorLabel,
    Guid PlotId,
    string PlotName,
    string PropertyName,
    Guid AcknowledgedBy,
    DateTimeOffset AcknowledgedAt);
