namespace TC.Agro.Analytics.Tests.Domain.Entities;

public class AlertTests
{
    [Fact]
    public void Alert_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sensorReadingId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var alert = new Alert
        {
            Id = id,
            SensorReadingId = sensorReadingId,
            SensorId = "SENSOR-001",
            PlotId = plotId,
            AlertType = AlertType.HighTemperature,
            Message = "High temperature detected: 38.5°C",
            Status = AlertStatus.Pending,
            Severity = AlertSeverity.High,
            Value = 38.5,
            Threshold = 35.0,
            CreatedAt = createdAt
        };

        // Assert
        alert.ShouldNotBeNull();
        alert.Id.ShouldBe(id);
        alert.SensorReadingId.ShouldBe(sensorReadingId);
        alert.SensorId.ShouldBe("SENSOR-001");
        alert.PlotId.ShouldBe(plotId);
        alert.AlertType.ShouldBe(AlertType.HighTemperature);
        alert.Message.ShouldBe("High temperature detected: 38.5°C");
        alert.Status.ShouldBe(AlertStatus.Pending);
        alert.Severity.ShouldBe(AlertSeverity.High);
        alert.Value.ShouldBe(38.5);
        alert.Threshold.ShouldBe(35.0);
        alert.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Alert_WithHighTemperatureType_ShouldSetCorrectType()
    {
        // Arrange & Act
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            AlertType = AlertType.HighTemperature,
            SensorId = "SENSOR-001",
            PlotId = Guid.NewGuid(),
            Status = AlertStatus.Pending,
            Severity = AlertSeverity.Critical,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.AlertType.ShouldBe(AlertType.HighTemperature);
        alert.AlertType.ShouldBe("HighTemperature");
    }

    [Fact]
    public void Alert_WithLowSoilMoistureType_ShouldSetCorrectType()
    {
        // Arrange & Act
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            AlertType = AlertType.LowSoilMoisture,
            SensorId = "SENSOR-002",
            PlotId = Guid.NewGuid(),
            Status = AlertStatus.Pending,
            Severity = AlertSeverity.High,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.AlertType.ShouldBe(AlertType.LowSoilMoisture);
        alert.AlertType.ShouldBe("LowSoilMoisture");
    }

    [Fact]
    public void Alert_WithLowBatteryType_ShouldSetCorrectType()
    {
        // Arrange & Act
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            AlertType = AlertType.LowBattery,
            SensorId = "SENSOR-003",
            PlotId = Guid.NewGuid(),
            Status = AlertStatus.Pending,
            Severity = AlertSeverity.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.AlertType.ShouldBe(AlertType.LowBattery);
        alert.AlertType.ShouldBe("LowBattery");
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Acknowledged")]
    [InlineData("Resolved")]
    public void Alert_WithDifferentStatuses_ShouldSetCorrectStatus(string status)
    {
        // Arrange & Act
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            SensorId = "SENSOR-001",
            PlotId = Guid.NewGuid(),
            AlertType = AlertType.HighTemperature,
            Status = status,
            Severity = AlertSeverity.High,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.Status.ShouldBe(status);
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public void Alert_WithDifferentSeverities_ShouldSetCorrectSeverity(string severity)
    {
        // Arrange & Act
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            SensorId = "SENSOR-001",
            PlotId = Guid.NewGuid(),
            AlertType = AlertType.HighTemperature,
            Status = AlertStatus.Pending,
            Severity = severity,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.Severity.ShouldBe(severity);
    }

    [Fact]
    public void Alert_WhenAcknowledged_ShouldSetAcknowledgedProperties()
    {
        // Arrange
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            SensorId = "SENSOR-001",
            PlotId = Guid.NewGuid(),
            AlertType = AlertType.HighTemperature,
            Status = AlertStatus.Pending,
            Severity = AlertSeverity.High,
            CreatedAt = DateTime.UtcNow
        };

        var acknowledgedAt = DateTime.UtcNow;
        var acknowledgedBy = "user@example.com";

        // Act
        alert.Status = AlertStatus.Acknowledged;
        alert.AcknowledgedAt = acknowledgedAt;
        alert.AcknowledgedBy = acknowledgedBy;

        // Assert
        alert.Status.ShouldBe(AlertStatus.Acknowledged);
        alert.AcknowledgedAt.ShouldBe(acknowledgedAt);
        alert.AcknowledgedBy.ShouldBe(acknowledgedBy);
    }

    [Fact]
    public void Alert_WhenResolved_ShouldSetResolvedProperties()
    {
        // Arrange
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            SensorId = "SENSOR-001",
            PlotId = Guid.NewGuid(),
            AlertType = AlertType.HighTemperature,
            Status = AlertStatus.Acknowledged,
            Severity = AlertSeverity.High,
            CreatedAt = DateTime.UtcNow,
            AcknowledgedAt = DateTime.UtcNow.AddMinutes(-10),
            AcknowledgedBy = "user@example.com"
        };

        var resolvedAt = DateTime.UtcNow;
        var resolvedBy = "admin@example.com";
        var resolutionNotes = "Temperature returned to normal levels";

        // Act
        alert.Status = AlertStatus.Resolved;
        alert.ResolvedAt = resolvedAt;
        alert.ResolvedBy = resolvedBy;
        alert.ResolutionNotes = resolutionNotes;

        // Assert
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.ResolvedAt.ShouldBe(resolvedAt);
        alert.ResolvedBy.ShouldBe(resolvedBy);
        alert.ResolutionNotes.ShouldBe(resolutionNotes);
    }

    [Fact]
    public void Alert_WithMetadata_ShouldStoreMetadataAsJson()
    {
        // Arrange
        var metadata = "{\"temperature\":38.5,\"humidity\":75.0}";

        // Act
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            SensorId = "SENSOR-001",
            PlotId = Guid.NewGuid(),
            AlertType = AlertType.HighTemperature,
            Status = AlertStatus.Pending,
            Severity = AlertSeverity.High,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.Metadata.ShouldBe(metadata);
        alert.Metadata.ShouldContain("temperature");
        alert.Metadata.ShouldContain("humidity");
    }

    [Fact]
    public void Alert_WithValueAndThreshold_ShouldStoreCorrectly()
    {
        // Arrange & Act
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            SensorId = "SENSOR-001",
            PlotId = Guid.NewGuid(),
            AlertType = AlertType.HighTemperature,
            Status = AlertStatus.Pending,
            Severity = AlertSeverity.Critical,
            Value = 45.0,
            Threshold = 35.0,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.Value.ShouldBe(45.0);
        alert.Threshold.ShouldBe(35.0);
        alert.Value.HasValue.ShouldBeTrue();
        alert.Threshold.HasValue.ShouldBeTrue();
        alert.Value.Value.ShouldBeGreaterThan(alert.Threshold.Value);
    }
}
