namespace TC.Agro.Analytics.Domain.Abstractions.Constants;

/// <summary>
/// Alert type constants for type safety
/// </summary>
public static class AlertTypes
{
    public const string HighTemperature = "HighTemperature";
    public const string LowSoilMoisture = "LowSoilMoisture";
    public const string LowBattery = "LowBattery";
}

/// <summary>
/// Alert status constants
/// </summary>
public static class AlertStatus
{
    public const string Pending = "Pending";
    public const string Acknowledged = "Acknowledged";
    public const string Resolved = "Resolved";
}

/// <summary>
/// Alert severity level constants
/// </summary>
public static class AlertSeverity
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Critical = "Critical";
}
