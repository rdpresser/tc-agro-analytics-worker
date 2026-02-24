namespace TC.Agro.Analytics.Service.Hubs;

public sealed record AlertResolvedNotification(
    Guid AlertId,
    Guid SensorId,
    string SensorLabel,
    Guid PlotId,
    string PlotName,
    string PropertyName,
    Guid ResolvedBy,
    string? ResolutionNotes,
    DateTimeOffset ResolvedAt);
