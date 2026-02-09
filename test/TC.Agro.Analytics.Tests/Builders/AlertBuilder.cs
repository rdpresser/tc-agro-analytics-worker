namespace TC.Agro.Analytics.Tests.Builders;

/// <summary>
/// Builder pattern for creating Alert instances in tests
/// </summary>
public class AlertBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _sensorReadingId = Guid.NewGuid();
    private string _sensorId = "SENSOR-TEST-001";
    private Guid _plotId = Guid.NewGuid();
    private string _alertType = AlertType.HighTemperature;
    private string _message = "Test alert message";
    private string _status = AlertStatus.Pending;
    private string _severity = AlertSeverity.High;
    private double? _value = 38.5;
    private double? _threshold = 35.0;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _acknowledgedAt;
    private string? _acknowledgedBy;
    private DateTime? _resolvedAt;
    private string? _resolvedBy;
    private string? _resolutionNotes;
    private string? _metadata;

    public AlertBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public AlertBuilder WithSensorReadingId(Guid sensorReadingId)
    {
        _sensorReadingId = sensorReadingId;
        return this;
    }

    public AlertBuilder WithSensorId(string sensorId)
    {
        _sensorId = sensorId;
        return this;
    }

    public AlertBuilder WithPlotId(Guid plotId)
    {
        _plotId = plotId;
        return this;
    }

    public AlertBuilder WithHighTemperature(double temperature, double threshold = 35.0)
    {
        _alertType = AlertType.HighTemperature;
        _message = $"High temperature detected: {temperature:F1}°C";
        _value = temperature;
        _threshold = threshold;
        return this;
    }

    public AlertBuilder WithLowSoilMoisture(double soilMoisture, double threshold = 20.0)
    {
        _alertType = AlertType.LowSoilMoisture;
        _message = $"Low soil moisture detected: {soilMoisture:F1}%";
        _value = soilMoisture;
        _threshold = threshold;
        return this;
    }

    public AlertBuilder WithLowBattery(double batteryLevel, double threshold = 15.0)
    {
        _alertType = AlertType.LowBattery;
        _message = $"Low battery warning: {batteryLevel:F1}%";
        _value = batteryLevel;
        _threshold = threshold;
        return this;
    }

    public AlertBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public AlertBuilder WithSeverity(string severity)
    {
        _severity = severity;
        return this;
    }

    public AlertBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public AlertBuilder AsAcknowledged(string acknowledgedBy, DateTime? acknowledgedAt = null)
    {
        _status = AlertStatus.Acknowledged;
        _acknowledgedBy = acknowledgedBy;
        _acknowledgedAt = acknowledgedAt ?? DateTime.UtcNow;
        return this;
    }

    public AlertBuilder AsResolved(string resolvedBy, string resolutionNotes, DateTime? resolvedAt = null)
    {
        _status = AlertStatus.Resolved;
        _resolvedBy = resolvedBy;
        _resolvedAt = resolvedAt ?? DateTime.UtcNow;
        _resolutionNotes = resolutionNotes;
        return this;
    }

    public AlertBuilder WithMetadata(string metadata)
    {
        _metadata = metadata;
        return this;
    }

    public Alert Build()
    {
        return new Alert
        {
            Id = _id,
            SensorReadingId = _sensorReadingId,
            SensorId = _sensorId,
            PlotId = _plotId,
            AlertType = _alertType,
            Message = _message,
            Status = _status,
            Severity = _severity,
            Value = _value,
            Threshold = _threshold,
            CreatedAt = _createdAt,
            AcknowledgedAt = _acknowledgedAt,
            AcknowledgedBy = _acknowledgedBy,
            ResolvedAt = _resolvedAt,
            ResolvedBy = _resolvedBy,
            ResolutionNotes = _resolutionNotes,
            Metadata = _metadata
        };
    }

    /// <summary>
    /// Creates a default pending HighTemperature alert
    /// </summary>
    public static Alert CreateDefaultHighTemperatureAlert() =>
        new AlertBuilder()
            .WithHighTemperature(40.0)
            .WithSeverity(AlertSeverity.Critical)
            .Build();

    /// <summary>
    /// Creates a default pending LowSoilMoisture alert
    /// </summary>
    public static Alert CreateDefaultLowSoilMoistureAlert() =>
        new AlertBuilder()
            .WithLowSoilMoisture(15.0)
            .WithSeverity(AlertSeverity.High)
            .Build();

    /// <summary>
    /// Creates a default pending LowBattery alert
    /// </summary>
    public static Alert CreateDefaultLowBatteryAlert() =>
        new AlertBuilder()
            .WithLowBattery(10.0)
            .WithSeverity(AlertSeverity.Medium)
            .Build();
}
