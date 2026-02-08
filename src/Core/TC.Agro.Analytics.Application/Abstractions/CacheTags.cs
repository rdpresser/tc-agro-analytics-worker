namespace TC.Agro.Analytics.Application.Abstractions;

/// <summary>
/// Cache tags for invalidation and cache key generation.
/// Used with FusionCache for efficient cache management.
/// </summary>
public static class CacheTags
{
    public const string Alerts = "alerts";
    public const string PendingAlerts = "alerts:pending";
    public const string SensorReadings = "sensor-readings";
    public const string PlotStatus = "plot-status";
    
    public static string AlertById(Guid alertId) => $"alert:{alertId}";
    public static string AlertsByPlot(Guid plotId) => $"alerts:plot:{plotId}";
    public static string AlertsBySensor(string sensorId) => $"alerts:sensor:{sensorId}";
    public static string PlotStatusById(Guid plotId) => $"plot-status:{plotId}";
    public static string SensorReadingById(Guid readingId) => $"sensor-reading:{readingId}";
}
