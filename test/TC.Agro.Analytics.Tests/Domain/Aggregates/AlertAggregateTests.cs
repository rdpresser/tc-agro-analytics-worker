namespace TC.Agro.Analytics.Tests.Domain.Aggregates;

/// <summary>
/// Unit tests for AlertAggregate
/// </summary>
public class AlertAggregateTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var sensorId = "SENSOR-001";
        var plotId = Guid.NewGuid();
        var type = AlertType.HighTemperature;
        var severity = AlertSeverity.High;
        var message = "High temperature detected";

        // Act
        var result = AlertAggregate.Create(
            sensorId,
            plotId,
            type,
            severity,
            message,
            temperature: 38.5);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var aggregate = result.Value;
        aggregate.ShouldNotBeNull();
        aggregate.SensorId.ShouldBe(sensorId);
        aggregate.PlotId.ShouldBe(plotId);
        aggregate.Type.ShouldBe(type);
        aggregate.Severity.ShouldBe(severity);
        aggregate.Message.ShouldBe(message);
        aggregate.Temperature.ShouldBe(38.5);
        aggregate.Status.ShouldBe(AlertStatus.Pending);
        aggregate.DetectedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseAlertCreatedDomainEvent()
    {
        // Arrange
        var sensorId = "SENSOR-001";
        var plotId = Guid.NewGuid();
        var type = AlertType.HighTemperature;
        var severity = AlertSeverity.Critical;
        var message = "Critical temperature alert";

        // Act
        var result = AlertAggregate.Create(
            sensorId,
            plotId,
            type,
            severity,
            message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var aggregate = result.Value;
        aggregate.UncommittedEvents.ShouldNotBeEmpty();
        aggregate.UncommittedEvents.Count.ShouldBe(1);

        var domainEvent = aggregate.UncommittedEvents[0];
        domainEvent.ShouldBeOfType<AlertAggregate.AlertCreatedDomainEvent>();
        
        var alertCreatedEvent = (AlertAggregate.AlertCreatedDomainEvent)domainEvent;
        alertCreatedEvent.SensorId.ShouldBe(sensorId);
        alertCreatedEvent.PlotId.ShouldBe(plotId);
        alertCreatedEvent.Type.ShouldBe(type);
        alertCreatedEvent.Severity.ShouldBe(severity);
        alertCreatedEvent.Message.ShouldBe(message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSensorId_ShouldFail(string? invalidSensorId)
    {
        // Arrange & Act
        var result = AlertAggregate.Create(
            invalidSensorId!,
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(e => e.Identifier == "SensorId.Required");
    }

    [Fact]
    public void Create_WithSensorIdTooLong_ShouldFail()
    {
        // Arrange
        var longSensorId = new string('A', 101);

        // Act
        var result = AlertAggregate.Create(
            longSensorId,
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "SensorId.TooLong");
    }

    [Fact]
    public void Create_WithEmptyPlotId_ShouldFail()
    {
        // Arrange & Act
        var result = AlertAggregate.Create(
            "SENSOR-001",
            Guid.Empty,
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test message");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "PlotId.Required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidMessage_ShouldFail(string? invalidMessage)
    {
        // Arrange & Act
        var result = AlertAggregate.Create(
            "SENSOR-001",
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            invalidMessage!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Message.Required");
    }

    [Fact]
    public void Create_WithMessageTooLong_ShouldFail()
    {
        // Arrange
        var longMessage = new string('A', 501);

        // Act
        var result = AlertAggregate.Create(
            "SENSOR-001",
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            longMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Message.TooLong");
    }

    #endregion

    #region Acknowledge Command Tests

    [Fact]
    public void Acknowledge_WithValidUserId_ShouldSucceed()
    {
        // Arrange
        var aggregate = CreateValidAlert();
        var userId = "user@example.com";

        // Act
        var result = aggregate.Acknowledge(userId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        aggregate.Status.ShouldBe(AlertStatus.Acknowledged);
        aggregate.AcknowledgedBy.ShouldBe(userId);
        aggregate.AcknowledgedAt.ShouldNotBeNull();
        aggregate.AcknowledgedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
    }

    [Fact]
    public void Acknowledge_WithValidUserId_ShouldRaiseAlertAcknowledgedDomainEvent()
    {
        // Arrange
        var aggregate = CreateValidAlert();
        aggregate.MarkEventsAsCommitted(); // Limpar eventos anteriores
        var userId = "user@example.com";

        // Act
        var result = aggregate.Acknowledge(userId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        aggregate.UncommittedEvents.ShouldNotBeEmpty();
        aggregate.UncommittedEvents.Count.ShouldBe(1);

        var domainEvent = aggregate.UncommittedEvents[0];
        domainEvent.ShouldBeOfType<AlertAggregate.AlertAcknowledgedDomainEvent>();
        
        var acknowledgedEvent = (AlertAggregate.AlertAcknowledgedDomainEvent)domainEvent;
        acknowledgedEvent.AcknowledgedBy.ShouldBe(userId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Acknowledge_WithInvalidUserId_ShouldFail(string? invalidUserId)
    {
        // Arrange
        var aggregate = CreateValidAlert();

        // Act
        var result = aggregate.Acknowledge(invalidUserId!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "UserId.Required");
        aggregate.Status.ShouldBe(AlertStatus.Pending);
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ShouldFail()
    {
        // Arrange
        var aggregate = CreateValidAlert();
        aggregate.Acknowledge("user1@example.com");

        // Act
        var result = aggregate.Acknowledge("user2@example.com");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.AlreadyAcknowledged");
    }

    [Fact]
    public void Acknowledge_WhenAlreadyResolved_ShouldFail()
    {
        // Arrange
        var aggregate = CreateValidAlert();
        aggregate.Acknowledge("user@example.com");
        aggregate.Resolve("admin@example.com", "Fixed");

        // Act
        var result = aggregate.Acknowledge("another@example.com");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.AlreadyResolved");
    }

    #endregion

    #region Resolve Command Tests

    [Fact]
    public void Resolve_WithValidData_ShouldSucceed()
    {
        // Arrange
        var aggregate = CreateValidAlert();
        aggregate.Acknowledge("user@example.com");
        var adminId = "admin@example.com";
        var resolutionNotes = "Temperature returned to normal";

        // Act
        var result = aggregate.Resolve(adminId, resolutionNotes);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        aggregate.Status.ShouldBe(AlertStatus.Resolved);
        aggregate.ResolvedBy.ShouldBe(adminId);
        aggregate.ResolutionNotes.ShouldBe(resolutionNotes);
        aggregate.ResolvedAt.ShouldNotBeNull();
        aggregate.ResolvedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
    }

    [Fact]
    public void Resolve_WithValidData_ShouldRaiseAlertResolvedDomainEvent()
    {
        // Arrange
        var aggregate = CreateValidAlert();
        aggregate.MarkEventsAsCommitted();
        var adminId = "admin@example.com";
        var resolutionNotes = "Issue resolved";

        // Act
        var result = aggregate.Resolve(adminId, resolutionNotes);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        aggregate.UncommittedEvents.ShouldNotBeEmpty();

        var domainEvent = aggregate.UncommittedEvents[aggregate.UncommittedEvents.Count - 1];
        domainEvent.ShouldBeOfType<AlertAggregate.AlertResolvedDomainEvent>();
        
        var resolvedEvent = (AlertAggregate.AlertResolvedDomainEvent)domainEvent;
        resolvedEvent.ResolvedBy.ShouldBe(adminId);
        resolvedEvent.ResolutionNotes.ShouldBe(resolutionNotes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithInvalidUserId_ShouldFail(string? invalidUserId)
    {
        // Arrange
        var aggregate = CreateValidAlert();

        // Act
        var result = aggregate.Resolve(invalidUserId!, "Notes");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "UserId.Required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithInvalidResolutionNotes_ShouldFail(string? invalidNotes)
    {
        // Arrange
        var aggregate = CreateValidAlert();

        // Act
        var result = aggregate.Resolve("admin@example.com", invalidNotes!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "ResolutionNotes.Required");
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_ShouldFail()
    {
        // Arrange
        var aggregate = CreateValidAlert();
        aggregate.Resolve("admin1@example.com", "First resolution");

        // Act
        var result = aggregate.Resolve("admin2@example.com", "Second resolution");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ValidationErrors.ShouldContain(e => e.Identifier == "Alert.AlreadyResolved");
    }

    [Fact]
    public void Resolve_FromPendingStatus_ShouldSucceed()
    {
        // Arrange
        var aggregate = CreateValidAlert();

        // Act
        var result = aggregate.Resolve("admin@example.com", "Resolved directly");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        aggregate.Status.ShouldBe(AlertStatus.Resolved);
    }

    #endregion

    #region Helper Methods

    private static AlertAggregate CreateValidAlert()
    {
        var result = AlertAggregate.Create(
            "SENSOR-001",
            Guid.NewGuid(),
            AlertType.HighTemperature,
            AlertSeverity.High,
            "Test alert message",
            temperature: 38.5);

        return result.Value;
    }

    #endregion
}
