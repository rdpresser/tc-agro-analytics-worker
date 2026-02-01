using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TC.Agro.Analytics.Domain.Entities;
using TC.Agro.Analytics.Infrastructure;
using TC.Agro.Analytics.Infrastructure.Projections;
using TC.Agro.Analytics.Tests.Builders;
using TC.Agro.SharedKernel.Infrastructure.Database;
using static TC.Agro.Analytics.Domain.Aggregates.SensorReadingAggregate;

namespace TC.Agro.Analytics.Tests.Infrastructure.Projections;

public class AlertProjectionHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AlertProjectionHandler> _logger;
    private readonly AlertProjectionHandler _sut;
    private readonly CultureInfo _originalCulture;

    public AlertProjectionHandlerTests()
    {
        // Save original culture and set to InvariantCulture for consistent number formatting
        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options, null!);
        _logger = A.Fake<ILogger<AlertProjectionHandler>>();
        _sut = new AlertProjectionHandler(_dbContext, _logger);
    }

    [Fact]
    public async Task Handle_HighTemperatureEvent_ShouldCreateAlert()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var sensorId = "SENSOR-001";
        var temperature = 40.0;
        var time = DateTime.UtcNow;

        var domainEvent = new HighTemperatureDetectedDomainEvent(
            aggregateId,
            sensorId,
            plotId,
            time,
            temperature,
            75.0,
            30.0,
            0.0,
            80.0,
            DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        alert.ShouldNotBeNull();
        alert.SensorReadingId.ShouldBe(aggregateId);
        alert.SensorId.ShouldBe(sensorId);
        alert.PlotId.ShouldBe(plotId);
        alert.AlertType.ShouldBe(AlertTypes.HighTemperature);
        alert.Status.ShouldBe(AlertStatus.Pending);
        alert.Value.ShouldBe(temperature);
        alert.Threshold.ShouldBe(35.0);
    }

    [Theory]
    [InlineData(36.0, AlertSeverity.Low)]
    [InlineData(40.0, AlertSeverity.Medium)]
    [InlineData(45.0, AlertSeverity.High)]
    [InlineData(55.0, AlertSeverity.Critical)]
    public async Task Handle_HighTemperatureEvent_ShouldSetCorrectSeverity(double temperature, string expectedSeverity)
    {
        // Arrange
        var domainEvent = new HighTemperatureDetectedDomainEvent(
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            DateTime.UtcNow,
            temperature,
            75.0,
            30.0,
            0.0,
            80.0,
            DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        alert.ShouldNotBeNull();
        alert.Severity.ShouldBe(expectedSeverity);
    }

    [Fact]
    public async Task Handle_LowSoilMoistureEvent_ShouldCreateAlert()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var sensorId = "SENSOR-002";
        var soilMoisture = 15.0;

        var domainEvent = new LowSoilMoistureDetectedDomainEvent(
            aggregateId,
            sensorId,
            plotId,
            DateTime.UtcNow,
            28.0,
            50.0,
            soilMoisture,
            0.0,
            75.0,
            DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        alert.ShouldNotBeNull();
        alert.SensorReadingId.ShouldBe(aggregateId);
        alert.SensorId.ShouldBe(sensorId);
        alert.PlotId.ShouldBe(plotId);
        alert.AlertType.ShouldBe(AlertTypes.LowSoilMoisture);
        alert.Status.ShouldBe(AlertStatus.Pending);
        alert.Value.ShouldBe(soilMoisture);
        alert.Threshold.ShouldBe(20.0);
    }

    [Fact]
    public async Task Handle_BatteryLowEvent_ShouldCreateAlert()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var sensorId = "SENSOR-003";
        var batteryLevel = 10.0;
        var threshold = 15.0;

        var domainEvent = new BatteryLowWarningDomainEvent(
            aggregateId,
            sensorId,
            plotId,
            batteryLevel,
            threshold,
            DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        alert.ShouldNotBeNull();
        alert.SensorReadingId.ShouldBe(aggregateId);
        alert.SensorId.ShouldBe(sensorId);
        alert.PlotId.ShouldBe(plotId);
        alert.AlertType.ShouldBe(AlertTypes.LowBattery);
        alert.Status.ShouldBe(AlertStatus.Pending);
        alert.Value.ShouldBe(batteryLevel);
        alert.Threshold.ShouldBe(threshold);
    }

    [Fact]
    public async Task Handle_HighTemperatureEvent_ShouldStoreMetadataAsJson()
    {
        // Arrange
        var domainEvent = new HighTemperatureDetectedDomainEvent(
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            DateTime.UtcNow,
            40.0,
            75.0,
            30.0,
            5.0,
            80.0,
            DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        alert.ShouldNotBeNull();
        alert.Metadata.ShouldNotBeNullOrEmpty();
        alert.Metadata.ShouldContain("Humidity");
        alert.Metadata.ShouldContain("75");
    }

    [Fact]
    public async Task Handle_MultipleEvents_ShouldCreateMultipleAlerts()
    {
        // Arrange
        var event1 = new HighTemperatureDetectedDomainEvent(
            Guid.NewGuid(), "SENSOR-001", Guid.NewGuid(),
            DateTime.UtcNow, 40.0, 75.0, 30.0, 0.0, 80.0, DateTimeOffset.UtcNow
        );

        var event2 = new LowSoilMoistureDetectedDomainEvent(
            Guid.NewGuid(), "SENSOR-002", Guid.NewGuid(),
            DateTime.UtcNow, 28.0, 50.0, 15.0, 0.0, 75.0, DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(event1, TestContext.Current.CancellationToken);
        await _sut.Handle(event2, TestContext.Current.CancellationToken);

        // Assert
        var alerts = await _dbContext.Alerts.ToListAsync(TestContext.Current.CancellationToken);

        alerts.Count.ShouldBe(2);
        alerts.Count(a => a.AlertType == AlertTypes.HighTemperature).ShouldBe(1);
        alerts.Count(a => a.AlertType == AlertTypes.LowSoilMoisture).ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedAtFromDomainEvent()
    {
        // Arrange
        var occurredOn = DateTimeOffset.UtcNow.AddMinutes(-5);
        
        var domainEvent = new HighTemperatureDetectedDomainEvent(
            Guid.NewGuid(), "SENSOR-001", Guid.NewGuid(),
            DateTime.UtcNow, 40.0, 75.0, 30.0, 0.0, 80.0, occurredOn
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        alert.ShouldNotBeNull();
        alert.CreatedAt.ShouldBe(occurredOn.DateTime);
    }

    [Fact]
    public async Task Handle_HighTemperatureEvent_MessageShouldContainTemperature()
    {
        // Arrange
        var temperature = 42.5;
        var domainEvent = new HighTemperatureDetectedDomainEvent(
            Guid.NewGuid(), "SENSOR-001", Guid.NewGuid(),
            DateTime.UtcNow, temperature, 75.0, 30.0, 0.0, 80.0, DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        alert.ShouldNotBeNull();
        alert.Message.ShouldContain("temperature");
        alert.Message.ShouldContain("42.5");
    }

    [Fact]
    public async Task Handle_LowSoilMoistureEvent_MessageShouldContainMoisture()
    {
        // Arrange
        var soilMoisture = 12.5;
        var domainEvent = new LowSoilMoistureDetectedDomainEvent(
            Guid.NewGuid(), "SENSOR-002", Guid.NewGuid(),
            DateTime.UtcNow, 28.0, 50.0, soilMoisture, 0.0, 75.0, DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        alert.ShouldNotBeNull();
        alert.Message.ShouldContain("moisture");
        alert.Message.ShouldContain("12.5");
    }

    [Fact]
    public async Task Handle_BatteryLowEvent_MessageShouldContainBatteryLevel()
    {
        // Arrange
        var batteryLevel = 8.5;
        var domainEvent = new BatteryLowWarningDomainEvent(
            Guid.NewGuid(), "SENSOR-003", Guid.NewGuid(),
            batteryLevel, 15.0, DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var alert = await _dbContext.Alerts.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        alert.ShouldNotBeNull();
        alert.Message.ShouldContain("battery");
        alert.Message.ShouldContain("8.5");
    }

    [Fact]
    public async Task Handle_AllAlerts_ShouldHaveStatusPending()
    {
        // Arrange
        var event1 = new HighTemperatureDetectedDomainEvent(
            Guid.NewGuid(), "SENSOR-001", Guid.NewGuid(),
            DateTime.UtcNow, 40.0, 75.0, 30.0, 0.0, 80.0, DateTimeOffset.UtcNow
        );
        var event2 = new LowSoilMoistureDetectedDomainEvent(
            Guid.NewGuid(), "SENSOR-002", Guid.NewGuid(),
            DateTime.UtcNow, 28.0, 50.0, 15.0, 0.0, 75.0, DateTimeOffset.UtcNow
        );
        var event3 = new BatteryLowWarningDomainEvent(
            Guid.NewGuid(), "SENSOR-003", Guid.NewGuid(),
            10.0, 15.0, DateTimeOffset.UtcNow
        );

        // Act
        await _sut.Handle(event1, TestContext.Current.CancellationToken);
        await _sut.Handle(event2, TestContext.Current.CancellationToken);
        await _sut.Handle(event3, TestContext.Current.CancellationToken);

        // Assert
        var alerts = await _dbContext.Alerts.ToListAsync(TestContext.Current.CancellationToken);
        alerts.Count.ShouldBe(3);
        alerts.All(a => a.Status == AlertStatus.Pending).ShouldBeTrue();
    }

    public void Dispose()
    {
        // Restore original culture
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalCulture;

        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
