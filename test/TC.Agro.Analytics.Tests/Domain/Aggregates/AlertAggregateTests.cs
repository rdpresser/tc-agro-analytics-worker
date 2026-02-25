namespace TC.Agro.Analytics.Tests.Domain.Aggregates;

/// <summary>
/// Unit tests for AlertAggregate.
/// Tests cover factory methods, business rules, state transitions, and domain events.
/// </summary>
public class AlertAggregateTests
{
    #region Create Factory Tests

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var type = AlertType.HighTemperature;
        var severity = AlertSeverity.High;
        var message = "High temperature detected: 38.5°C";
        var value = 38.5;
        var threshold = 35.0;

        // Act
        var result = AlertAggregate.Create(sensorId, type, severity, message, value, threshold);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SensorId.ShouldBe(sensorId);
        result.Value.Type.ShouldBe(type);
        result.Value.Severity.ShouldBe(severity);
        result.Value.Message.ShouldBe(message);
        result.Value.Value.ShouldBe(value);
        result.Value.Threshold.ShouldBe(threshold);
        result.Value.Status.ShouldBe(AlertStatus.Pending);
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.IsActive.ShouldBeTrue();
        result.Value.CreatedAt.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow.AddSeconds(2));
    }

    [Fact]
    public void Create_WithMetadata_ShouldSucceed()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var metadata = "{\"humidity\":65.0,\"soilMoisture\":45.0}";

        // Act
        var result = AlertAggregate.Create(
            sensorId,
            AlertType.HighTemperature,
            AlertSeverity.High,
            "High temperature detected",
            38.5,
            35.0,
            metadata);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Metadata.ShouldBe(metadata);
    }

    [Fact]
    public void Create_WithEmptySensorId_ShouldReturnValidationError()
    {
        // Arrange
        var sensorId = Guid.Empty;

        // Act
        var result = AlertAggregate.Create(
            sensorId,
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "SensorId.Required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidMessage_ShouldReturnValidationError(string? message)
    {
        // Act
        var result = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            message!,
            38.5,
            35.0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Message.Required");
    }

    [Fact]
    public void Create_WithMessageTooLong_ShouldReturnValidationError()
    {
        // Arrange
        var longMessage = new string('A', 501); // 501 characters

        // Act
        var result = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            longMessage,
            38.5,
            35.0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Message.TooLong");
    }

    [Theory]
    [InlineData(38.5, 35.0)]
    [InlineData(100.0, 50.0)]
    [InlineData(0.5, 0.1)]
    public void Create_WithVariousValueAndThreshold_ShouldSucceed(double value, double threshold)
    {
        // Act
        var result = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            value,
            threshold);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.Threshold.ShouldBe(threshold);
    }

    #endregion

    #region CreateFromSensorData Tests

    [Fact]
    public void CreateFromSensorData_WithHighTemperature_ShouldCreateAlert()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var temperature = 40.0;

        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: sensorId,
            temperature: temperature,
            soilMoisture: 30.0,
            batteryLevel: 80.0,
            humidity: 60.0,
            rainfall: 0.0,
            maxTemperature: 35.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Type.ShouldBe(AlertType.HighTemperature);
        result.Value[0].SensorId.ShouldBe(sensorId);
        result.Value[0].Value.ShouldBe(temperature);
        result.Value[0].Threshold.ShouldBe(35.0);
        result.Value[0].Metadata.ShouldNotBeNull();
    }

    [Fact]
    public void CreateFromSensorData_WithLowSoilMoisture_ShouldCreateAlert()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var soilMoisture = 15.0;

        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: sensorId,
            temperature: 25.0,
            soilMoisture: soilMoisture,
            batteryLevel: 80.0,
            humidity: 50.0,
            rainfall: 0.0,
            minSoilMoisture: 20.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Type.ShouldBe(AlertType.LowSoilMoisture);
        result.Value[0].Value.ShouldBe(soilMoisture);
        result.Value[0].Threshold.ShouldBe(20.0);
    }

    [Fact]
    public void CreateFromSensorData_WithLowBattery_ShouldCreateAlert()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var batteryLevel = 10.0;

        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: sensorId,
            temperature: 25.0,
            soilMoisture: 30.0,
            batteryLevel: batteryLevel,
            humidity: 50.0,
            rainfall: 0.0,
            minBatteryLevel: 15.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Type.ShouldBe(AlertType.LowBattery);
        result.Value[0].Value.ShouldBe(batteryLevel);
        result.Value[0].Threshold.ShouldBe(15.0);
    }

    [Fact]
    public void CreateFromSensorData_WithMultipleViolations_ShouldCreateMultipleAlerts()
    {
        // Arrange
        var sensorId = Guid.NewGuid();

        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: sensorId,
            temperature: 45.0,       // High temperature
            soilMoisture: 10.0,      // Low soil moisture
            batteryLevel: 8.0,       // Low battery
            humidity: 70.0,
            rainfall: 0.0,
            maxTemperature: 35.0,
            minSoilMoisture: 20.0,
            minBatteryLevel: 15.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(3);
        result.Value.ShouldContain(a => a.Type == AlertType.HighTemperature);
        result.Value.ShouldContain(a => a.Type == AlertType.LowSoilMoisture);
        result.Value.ShouldContain(a => a.Type == AlertType.LowBattery);
    }

    [Fact]
    public void CreateFromSensorData_WithNoViolations_ShouldReturnEmptyList()
    {
        // Arrange
        var sensorId = Guid.NewGuid();

        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: sensorId,
            temperature: 25.0,       // OK
            soilMoisture: 40.0,      // OK
            batteryLevel: 80.0,      // OK
            humidity: 50.0,
            rainfall: 0.0,
            maxTemperature: 35.0,
            minSoilMoisture: 20.0,
            minBatteryLevel: 15.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public void CreateFromSensorData_WithNullValues_ShouldNotCreateAlerts()
    {
        // Arrange
        var sensorId = Guid.NewGuid();

        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: sensorId,
            temperature: null,
            soilMoisture: null,
            batteryLevel: null,
            humidity: null,
            rainfall: null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public void CreateFromSensorData_WithEmptySensorId_ShouldReturnValidationError()
    {
        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: Guid.Empty,
            temperature: 40.0,
            soilMoisture: 30.0,
            batteryLevel: 80.0,
            humidity: 60.0,
            rainfall: 0.0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "SensorId.Required");
    }

    [Theory]
    [InlineData(50.0, 35.0)] // excess = 15 -> Critical
    [InlineData(45.0, 35.0)] // excess = 10 -> High
    [InlineData(40.0, 35.0)] // excess = 5  -> Medium
    [InlineData(37.0, 35.0)] // excess = 2  -> Low
    public void CreateFromSensorData_TemperatureSeverity_ShouldBeCalculatedCorrectly(
        double temperature,
        double threshold)
    {
        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: Guid.NewGuid(),
            temperature: temperature,
            soilMoisture: 30.0,
            batteryLevel: 80.0,
            humidity: 60.0,
            rainfall: 0.0,
            maxTemperature: threshold);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        var alert = result.Value[0];

        var excess = temperature - threshold;
        var expectedSeverity = excess switch
        {
            >= 15 => AlertSeverity.Critical,
            >= 10 => AlertSeverity.High,
            >= 5 => AlertSeverity.Medium,
            _ => AlertSeverity.Low
        };

        alert.Severity.ShouldBe(expectedSeverity);
    }

    [Theory]
    [InlineData(5.0, 35.0)]   // deficit = 30 -> Critical
    [InlineData(10.0, 30.0)]  // deficit = 20 -> High
    [InlineData(20.0, 30.0)]  // deficit = 10 -> Medium
    [InlineData(28.0, 30.0)]  // deficit = 2  -> Low
    public void CreateFromSensorData_SoilMoistureSeverity_ShouldBeCalculatedCorrectly(
        double soilMoisture,
        double threshold)
    {
        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: Guid.NewGuid(),
            temperature: 25.0,
            soilMoisture: soilMoisture,
            batteryLevel: 80.0,
            humidity: 50.0,
            rainfall: 0.0,
            minSoilMoisture: threshold);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        var alert = result.Value[0];

        var deficit = threshold - soilMoisture;
        var expectedSeverity = deficit switch
        {
            >= 30 => AlertSeverity.Critical,
            >= 20 => AlertSeverity.High,
            >= 10 => AlertSeverity.Medium,
            _ => AlertSeverity.Low
        };

        alert.Severity.ShouldBe(expectedSeverity);
    }

    [Theory]
    [InlineData(5.0)]   // < 10 -> Critical
    [InlineData(15.0)]  // < 20 -> High
    [InlineData(25.0)]  // < 30 -> Medium
    [InlineData(35.0)]  // >= 30 -> Low
    public void CreateFromSensorData_BatterySeverity_ShouldBeCalculatedCorrectly(double batteryLevel)
    {
        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: Guid.NewGuid(),
            temperature: 25.0,
            soilMoisture: 40.0,
            batteryLevel: batteryLevel,
            humidity: 50.0,
            rainfall: 0.0,
            minBatteryLevel: 100.0); // High threshold to ensure alert

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        var alert = result.Value[0];

        var expectedSeverity = batteryLevel switch
        {
            < 10 => AlertSeverity.Critical,
            < 20 => AlertSeverity.High,
            < 30 => AlertSeverity.Medium,
            _ => AlertSeverity.Low
        };

        alert.Severity.ShouldBe(expectedSeverity);
    }

    #endregion

    #region Acknowledge Tests

    [Fact]
    public void Acknowledge_WithValidUserId_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "High temperature detected",
            38.5,
            35.0).Value;
        var userId = Guid.NewGuid();

        // Act
        var result = alert.Acknowledge(userId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Acknowledged);
        alert.AcknowledgedAt.ShouldNotBeNull();
        alert.AcknowledgedBy.ShouldBe(userId);
        alert.AcknowledgedAt.Value.ShouldBeInRange(
            DateTimeOffset.UtcNow.AddSeconds(-5),
            DateTimeOffset.UtcNow);
        alert.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Acknowledge_WithInvalidUserId_ShouldReturnValidationError()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Acknowledge(Guid.Empty);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "UserId.Required");
        alert.Status.ShouldBe(AlertStatus.Pending); // Should not change
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ShouldReturnValidationError()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        var firstUserId = Guid.NewGuid();
        alert.Acknowledge(firstUserId);
        var firstAcknowledgedAt = alert.AcknowledgedAt;

        // Act
        var result = alert.Acknowledge(Guid.NewGuid());

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.NotPending");
        alert.AcknowledgedBy.ShouldBe(firstUserId); // Should not change
        alert.AcknowledgedAt.ShouldBe(firstAcknowledgedAt); // Should not change
    }

    [Fact]
    public void Acknowledge_WhenResolved_ShouldReturnValidationError()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        alert.Resolve(Guid.NewGuid(), "Already resolved");

        // Act
        var result = alert.Acknowledge(Guid.NewGuid());

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.NotPending");
        alert.Status.ShouldBe(AlertStatus.Resolved); // Should remain resolved
    }

    #endregion

    #region Resolve Tests

    [Fact]
    public void Resolve_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "High temperature detected",
            38.5,
            35.0).Value;
        var userId = Guid.NewGuid();
        var notes = "Irrigation activated";

        // Act
        var result = alert.Resolve(userId, notes);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.ResolvedAt.ShouldNotBeNull();
        alert.ResolvedBy.ShouldBe(userId);
        alert.ResolutionNotes.ShouldBe(notes);
        alert.ResolvedAt.Value.ShouldBeInRange(
            DateTimeOffset.UtcNow.AddSeconds(-5),
            DateTimeOffset.UtcNow);
        alert.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Resolve_FromPendingStatus_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Resolve(Guid.NewGuid(), "Fixed directly");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.AcknowledgedAt.ShouldBeNull(); // Never acknowledged
    }

    [Fact]
    public void Resolve_FromAcknowledgedStatus_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        var acknowledgeUserId = Guid.NewGuid();
        alert.Acknowledge(acknowledgeUserId);
        var acknowledgedAt = alert.AcknowledgedAt;

        // Act
        var resolveUserId = Guid.NewGuid();
        var result = alert.Resolve(resolveUserId, "Issue fixed");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.AcknowledgedAt.ShouldBe(acknowledgedAt); // Should preserve
        alert.AcknowledgedBy.ShouldBe(acknowledgeUserId); // Should preserve
    }

    [Fact]
    public void Resolve_WithInvalidUserId_ShouldReturnValidationError()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Resolve(Guid.Empty, "notes");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "UserId.Required");
        alert.Status.ShouldBe(AlertStatus.Pending); // Should not change
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_ShouldReturnValidationError()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        var firstUserId = Guid.NewGuid();
        var firstNotes = "First resolution";
        alert.Resolve(firstUserId, firstNotes);
        var firstResolvedAt = alert.ResolvedAt;

        // Act
        var result = alert.Resolve(Guid.NewGuid(), "Second resolution attempt");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.AlreadyResolved");
        alert.ResolvedBy.ShouldBe(firstUserId); // Should not change
        alert.ResolutionNotes.ShouldBe(firstNotes); // Should not change
        alert.ResolvedAt.ShouldBe(firstResolvedAt); // Should not change
    }

    [Fact]
    public void Resolve_WithoutNotes_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Resolve(Guid.NewGuid(), null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.ResolutionNotes.ShouldBeNull();
    }

    [Fact]
    public void Resolve_WithEmptyNotes_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Resolve(Guid.NewGuid(), string.Empty);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.ResolutionNotes.ShouldBe(string.Empty);
    }

    #endregion

    #region Domain Events Tests

    [Fact]
    public void Create_ShouldRaiseAlertCreatedDomainEvent()
    {
        // Arrange & Act
        var result = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "High temp",
            38.5,
            35.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var events = result.Value.UncommittedEvents.ToList();
        events.ShouldNotBeEmpty();
        events.Count.ShouldBe(1);

        var createdEvent = events.OfType<AlertAggregate.AlertCreatedDomainEvent>().FirstOrDefault();
        createdEvent.ShouldNotBeNull();
        createdEvent.AggregateId.ShouldBe(result.Value.Id);
        createdEvent.SensorId.ShouldBe(result.Value.SensorId);
        createdEvent.Type.ShouldBe(AlertType.HighTemperature);
        createdEvent.Severity.ShouldBe(AlertSeverity.High);
        createdEvent.Message.ShouldBe("High temp");
        createdEvent.Value.ShouldBe(38.5);
        createdEvent.Threshold.ShouldBe(35.0);
    }

    [Fact]
    public void Acknowledge_ShouldRaiseAlertAcknowledgedDomainEvent()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        var userId = Guid.NewGuid();

        // Act
        alert.Acknowledge(userId);

        // Assert
        var events = alert.UncommittedEvents.ToList();
        events.Count.ShouldBe(2); // Created + Acknowledged

        var acknowledgedEvent = events.OfType<AlertAggregate.AlertAcknowledgedDomainEvent>().FirstOrDefault();
        acknowledgedEvent.ShouldNotBeNull();
        acknowledgedEvent.AggregateId.ShouldBe(alert.Id);
        acknowledgedEvent.AcknowledgedBy.ShouldBe(userId);
        acknowledgedEvent.AcknowledgedAt.ShouldBeInRange(
            DateTimeOffset.UtcNow.AddSeconds(-5),
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Resolve_ShouldRaiseAlertResolvedDomainEvent()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        var userId = Guid.NewGuid();
        var notes = "Fixed";

        // Act
        alert.Resolve(userId, notes);

        // Assert
        var events = alert.UncommittedEvents.ToList();
        events.Count.ShouldBe(2); // Created + Resolved

        var resolvedEvent = events.OfType<AlertAggregate.AlertResolvedDomainEvent>().FirstOrDefault();
        resolvedEvent.ShouldNotBeNull();
        resolvedEvent.AggregateId.ShouldBe(alert.Id);
        resolvedEvent.ResolvedBy.ShouldBe(userId);
        resolvedEvent.ResolutionNotes.ShouldBe(notes);
        resolvedEvent.ResolvedAt.ShouldBeInRange(
            DateTimeOffset.UtcNow.AddSeconds(-5),
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void CompleteLifecycle_ShouldRaiseAllDomainEvents()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act - Complete lifecycle
        alert.Acknowledge(Guid.NewGuid());
        alert.Resolve(Guid.NewGuid(), "All done");

        // Assert
        var events = alert.UncommittedEvents.ToList();
        events.Count.ShouldBe(3); // Created + Acknowledged + Resolved
        events.OfType<AlertAggregate.AlertCreatedDomainEvent>().Count().ShouldBe(1);
        events.OfType<AlertAggregate.AlertAcknowledgedDomainEvent>().Count().ShouldBe(1);
        events.OfType<AlertAggregate.AlertResolvedDomainEvent>().Count().ShouldBe(1);
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void StateTransition_PendingToAcknowledgedToResolved_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test",
            38.5,
            35.0).Value;

        // Assert initial state
        alert.Status.ShouldBe(AlertStatus.Pending);
        alert.AcknowledgedAt.ShouldBeNull();
        alert.ResolvedAt.ShouldBeNull();

        // Act - Acknowledge
        alert.Acknowledge(Guid.NewGuid());

        // Assert acknowledged state
        alert.Status.ShouldBe(AlertStatus.Acknowledged);
        alert.AcknowledgedAt.ShouldNotBeNull();
        alert.ResolvedAt.ShouldBeNull();

        // Act - Resolve
        alert.Resolve(Guid.NewGuid(), "Done");

        // Assert resolved state
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.AcknowledgedAt.ShouldNotBeNull();
        alert.ResolvedAt.ShouldNotBeNull();
    }

    [Fact]
    public void StateTransition_PendingToResolved_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test",
            38.5,
            35.0).Value;

        // Assert initial state
        alert.Status.ShouldBe(AlertStatus.Pending);

        // Act - Resolve directly
        alert.Resolve(Guid.NewGuid(), "Fixed immediately");

        // Assert
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.AcknowledgedAt.ShouldBeNull(); // Skipped acknowledgment
        alert.ResolvedAt.ShouldNotBeNull();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void Create_WithExactThreshold_ShouldNotTriggerAlert()
    {
        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: Guid.NewGuid(),
            temperature: 35.0,  // Exactly at threshold
            soilMoisture: 20.0, // Exactly at threshold
            batteryLevel: 15.0, // Exactly at threshold
            humidity: 50.0,
            rainfall: 0.0,
            maxTemperature: 35.0,
            minSoilMoisture: 20.0,
            minBatteryLevel: 15.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty(); // No alerts at exact threshold
    }

    [Fact]
    public void Create_WithJustAboveThreshold_ShouldTriggerAlert()
    {
        // Act
        var result = AlertAggregate.CreateFromSensorData(
            sensorId: Guid.NewGuid(),
            temperature: 35.1,  // Just above threshold
            soilMoisture: 30.0,
            batteryLevel: 80.0,
            humidity: 50.0,
            rainfall: 0.0,
            maxTemperature: 35.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Type.ShouldBe(AlertType.HighTemperature);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-10.0)]
    [InlineData(100.0)]
    [InlineData(1000.0)]
    public void Create_WithExtremeValues_ShouldHandleCorrectly(double value)
    {
        // Act
        var result = AlertAggregate.Create(
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.Critical,
            "Extreme value test",
            value,
            35.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    #endregion
}
