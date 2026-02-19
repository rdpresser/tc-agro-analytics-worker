namespace TC.Agro.Analytics.Tests.Domain.Aggregates;

/// <summary>
/// Unit tests for AlertAggregate.
/// </summary>
public class AlertAggregateTests
{
    #region Factory Tests

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var type = AlertType.HighTemperature;
        var severity = AlertSeverity.High;
        var message = "High temperature detected: 38.5°C";
        var value = 38.5;
        var threshold = 35.0;

        // Act
        var result = AlertAggregate.Create(sensorId, plotId, type, severity, message, value, threshold);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SensorId.ShouldBe(sensorId);
        result.Value.PlotId.ShouldBe(plotId);
        result.Value.Type.ShouldBe(type);
        result.Value.Severity.ShouldBe(severity);
        result.Value.Message.ShouldBe(message);
        result.Value.Value.ShouldBe(value);
        result.Value.Threshold.ShouldBe(threshold);
        result.Value.Status.ShouldBe(AlertStatus.Pending);
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithEmptySensorId_ShouldReturnValidationError()
    {
        // Arrange
        var sensorId = Guid.Empty;
        var plotId = Guid.NewGuid();

        // Act
        var result = AlertAggregate.Create(sensorId, plotId, AlertType.HighTemperature, AlertSeverity.High, "Test message", 38.5, 35.0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "SensorId.Required");
    }

    [Fact]
    public void Create_WithEmptyPlotId_ShouldReturnValidationError()
    {
        // Act
        var result = AlertAggregate.Create(Guid.NewGuid(), Guid.Empty, AlertType.HighTemperature, AlertSeverity.High, "Test message", 38.5, 35.0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "PlotId.Required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidMessage_ShouldReturnValidationError(string message)
    {
        // Act
        var result = AlertAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), AlertType.HighTemperature, AlertSeverity.High, message, 38.5, 35.0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Message.Required");
    }

    #endregion

    #region Acknowledge Tests

    [Fact]
    public void Acknowledge_WithValidUserId_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "High temperature detected",
            38.5,
            35.0).Value;
        var userId = "user@example.com";

        // Act
        var result = alert.Acknowledge(userId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Acknowledged);
        alert.AcknowledgedAt.ShouldNotBeNull();
        alert.AcknowledgedBy.ShouldBe(userId);
        alert.AcknowledgedAt.Value.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-5), DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Acknowledge_WithInvalidUserId_ShouldReturnValidationError(string userId)
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Acknowledge(userId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "UserId.Required");
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ShouldReturnValidationError()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;
        alert.Acknowledge("user1@example.com");

        // Act
        var result = alert.Acknowledge("user2@example.com");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.NotPending");
    }

    #endregion

    #region Resolve Tests

    [Fact]
    public void Resolve_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "High temperature detected",
            38.5,
            35.0).Value;
        var userId = "user@example.com";
        var notes = "Irrigation activated";

        // Act
        var result = alert.Resolve(userId, notes);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.ResolvedAt.ShouldNotBeNull();
        alert.ResolvedBy.ShouldBe(userId);
        alert.ResolutionNotes.ShouldBe(notes);
        alert.ResolvedAt.Value.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-5), DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Resolve_FromAcknowledgedStatus_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;
        alert.Acknowledge("user@example.com");

        // Act
        var result = alert.Resolve("user@example.com", "Issue fixed");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithInvalidUserId_ShouldReturnValidationError(string userId)
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Resolve(userId, "notes");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "UserId.Required");
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_ShouldReturnValidationError()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;
        alert.Resolve("user1@example.com", "First resolution");

        // Act
        var result = alert.Resolve("user2@example.com", "Second resolution attempt");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.AlreadyResolved");
    }

    [Fact]
    public void Resolve_WithoutNotes_ShouldSucceed()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        var result = alert.Resolve("user@example.com", null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        alert.Status.ShouldBe(AlertStatus.Resolved);
        alert.ResolutionNotes.ShouldBeNull();
    }

    #endregion

    #region Domain Events Tests

    [Fact]
    public void Create_ShouldRaiseAlertCreatedDomainEvent()
    {
        // Arrange & Act
        var result = AlertAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), AlertType.HighTemperature, AlertSeverity.High, "High temp", 38.5, 35.0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var events = result.Value.UncommittedEvents.ToList();
        events.ShouldNotBeEmpty();
        events.ShouldContain(e => e is AlertAggregate.AlertCreatedDomainEvent);
    }

    [Fact]
    public void Acknowledge_ShouldRaiseAlertAcknowledgedDomainEvent()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        alert.Acknowledge("user@example.com");

        // Assert
        var events = alert.UncommittedEvents.ToList();
        events.Count.ShouldBeGreaterThan(1);
        events.ShouldContain(e => e is AlertAggregate.AlertAcknowledgedDomainEvent);
    }

    [Fact]
    public void Resolve_ShouldRaiseAlertResolvedDomainEvent()
    {
        // Arrange
        var alert = AlertAggregate.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message",
            38.5,
            35.0).Value;

        // Act
        alert.Resolve("user@example.com", "Fixed");

        // Assert
        var events = alert.UncommittedEvents.ToList();
        events.Count.ShouldBeGreaterThan(1);
        events.ShouldContain(e => e is AlertAggregate.AlertResolvedDomainEvent);
    }

    #endregion
}
