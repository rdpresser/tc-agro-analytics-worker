namespace TC.Agro.Analytics.Application.UseCases.ProcessSensorAlerts;

/// <summary>
/// Command for processing sensor alerts based on ingested sensor data.
/// </summary>
public sealed record ProcessSensorAlertsCommand(
    Guid SensorId,
    Guid PlotId,
    DateTime Time,
    double? Temperature,
    double? Humidity,
    double? SoilMoisture,
    double? Rainfall,
    double? BatteryLevel);
