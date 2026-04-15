using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TC.Agro.Analytics.Application.Abstractions.Options.AlertThreshold;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.MessageBrokerHandlers;
using TC.Agro.Contracts.Events.SensorIngested;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.Messaging;

namespace TC.Agro.Analytics.Tests.Application.Handlers;

public sealed class SensorIngestedHandlerTests
{
    private readonly IAlertAggregateRepository _alertRepository = A.Fake<IAlertAggregateRepository>();
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IAlertHubNotifier _hubNotifier = A.Fake<IAlertHubNotifier>();

    private readonly AlertThresholdOptions _defaultThresholds = new()
    {
        MaxTemperature = 35,
        MinSoilMoisture = 30,
        MinBatteryLevel = 20
    };

    // ──────────────────────────────────────────
    // Guard clause
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEventIsNull_ShouldThrowArgumentNullException()
    {
        var handler = CreateHandler();

        await Should.ThrowAsync<ArgumentNullException>(
            () => handler.Handle(null!, CancellationToken.None));
    }

    // ──────────────────────────────────────────
    // No alerts — all values within thresholds
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAllMetricsAreNormal_ShouldPersistNoAlertsAndNotNotify()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildEvent(temperature: 25, soilMoisture: 45, batteryLevel: 80);

        var handler = CreateHandler();
        await handler.Handle(@event, ct);

        A.CallTo(() => _alertRepository.AddRange(A<IEnumerable<AlertAggregate>>._))
            .MustHaveHappenedOnceExactly(); // AddRange is always called (may be empty list)

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _hubNotifier.NotifyAlertCreatedAsync(
            A<Guid>._, A<Guid>._, A<string>._, A<string>._, A<string>._,
            A<double>._, A<double>._, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // High temperature alarm
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenTemperatureExceedsThreshold_ShouldAddAlertAndNotify()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();
        var @event = BuildEvent(sensorId: sensorId, temperature: 40, soilMoisture: 45, batteryLevel: 80);

        var capturedAlerts = new List<AlertAggregate>();

        A.CallTo(() => _alertRepository.AddRange(A<IEnumerable<AlertAggregate>>._))
            .Invokes(call => capturedAlerts.AddRange(call.GetArgument<IEnumerable<AlertAggregate>>(0)!));

        var handler = CreateHandler();
        await handler.Handle(@event, ct);

        capturedAlerts.ShouldNotBeEmpty();
        capturedAlerts.ShouldContain(a => a.Type == AlertType.HighTemperature);
        capturedAlerts.ShouldContain(a => a.SensorId == sensorId);

        A.CallTo(() => _hubNotifier.NotifyAlertCreatedAsync(
            A<Guid>._, sensorId, A<string>._, A<string>._, A<string>._,
            A<double>._, A<double>._, A<DateTimeOffset>._))
            .MustHaveHappened();
    }

    // ──────────────────────────────────────────
    // Low soil moisture alarm
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenSoilMoistureBelowThreshold_ShouldAddAlertAndNotify()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();
        var @event = BuildEvent(sensorId: sensorId, temperature: 25, soilMoisture: 10, batteryLevel: 80);

        var capturedAlerts = new List<AlertAggregate>();

        A.CallTo(() => _alertRepository.AddRange(A<IEnumerable<AlertAggregate>>._))
            .Invokes(call => capturedAlerts.AddRange(call.GetArgument<IEnumerable<AlertAggregate>>(0)!));

        var handler = CreateHandler();
        await handler.Handle(@event, ct);

        capturedAlerts.ShouldContain(a => a.Type == AlertType.LowSoilMoisture);

        A.CallTo(() => _hubNotifier.NotifyAlertCreatedAsync(
            A<Guid>._, sensorId, A<string>._, A<string>._, A<string>._,
            A<double>._, A<double>._, A<DateTimeOffset>._))
            .MustHaveHappened();
    }

    // ──────────────────────────────────────────
    // Low battery alarm
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenBatteryBelowThreshold_ShouldAddAlertAndNotify()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();
        var @event = BuildEvent(sensorId: sensorId, temperature: 25, soilMoisture: 45, batteryLevel: 10);

        var capturedAlerts = new List<AlertAggregate>();

        A.CallTo(() => _alertRepository.AddRange(A<IEnumerable<AlertAggregate>>._))
            .Invokes(call => capturedAlerts.AddRange(call.GetArgument<IEnumerable<AlertAggregate>>(0)!));

        var handler = CreateHandler();
        await handler.Handle(@event, ct);

        capturedAlerts.ShouldContain(a => a.Type == AlertType.LowBattery);

        A.CallTo(() => _hubNotifier.NotifyAlertCreatedAsync(
            A<Guid>._, sensorId, A<string>._, A<string>._, A<string>._,
            A<double>._, A<double>._, A<DateTimeOffset>._))
            .MustHaveHappened();
    }

    // ──────────────────────────────────────────
    // Multiple simultaneous alerts
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenMultipleThresholdsBreached_ShouldGenerateMultipleAlertsAndNotifyForEach()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();

        // All three thresholds breached
        var @event = BuildEvent(sensorId: sensorId, temperature: 50, soilMoisture: 5, batteryLevel: 5);

        var capturedAlerts = new List<AlertAggregate>();

        A.CallTo(() => _alertRepository.AddRange(A<IEnumerable<AlertAggregate>>._))
            .Invokes(call => capturedAlerts.AddRange(call.GetArgument<IEnumerable<AlertAggregate>>(0)!));

        var handler = CreateHandler();
        await handler.Handle(@event, ct);

        capturedAlerts.Count.ShouldBe(3);

        A.CallTo(() => _hubNotifier.NotifyAlertCreatedAsync(
            A<Guid>._, A<Guid>._, A<string>._, A<string>._, A<string>._,
            A<double>._, A<double>._, A<DateTimeOffset>._))
            .MustHaveHappenedANumberOfTimesMatching(n => n == 3);
    }

    // ──────────────────────────────────────────
    // Null metrics — no alerts
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAllMetricsAreNull_ShouldNotGenerateAnyAlerts()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildEvent(temperature: null, soilMoisture: null, batteryLevel: null);

        var capturedAlerts = new List<AlertAggregate>();

        A.CallTo(() => _alertRepository.AddRange(A<IEnumerable<AlertAggregate>>._))
            .Invokes(call => capturedAlerts.AddRange(call.GetArgument<IEnumerable<AlertAggregate>>(0)!));

        var handler = CreateHandler();
        await handler.Handle(@event, ct);

        capturedAlerts.ShouldBeEmpty();

        A.CallTo(() => _hubNotifier.NotifyAlertCreatedAsync(
            A<Guid>._, A<Guid>._, A<string>._, A<string>._, A<string>._,
            A<double>._, A<double>._, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // SaveChanges always called once
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_Always_ShouldCallSaveChangesOnce()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildEvent(temperature: 25, soilMoisture: 45, batteryLevel: 80);

        var handler = CreateHandler();
        await handler.Handle(@event, ct);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct))
            .MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // Custom threshold respected
    // ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WithCustomHighThreshold_ShouldRespectConfiguredMaxTemperature()
    {
        var ct = TestContext.Current.CancellationToken;
        var customThresholds = new AlertThresholdOptions { MaxTemperature = 50, MinSoilMoisture = 30, MinBatteryLevel = 20 };

        // Temperature = 40, just below custom threshold of 50 → no alert
        var @event = BuildEvent(temperature: 40, soilMoisture: 45, batteryLevel: 80);

        var capturedAlerts = new List<AlertAggregate>();

        A.CallTo(() => _alertRepository.AddRange(A<IEnumerable<AlertAggregate>>._))
            .Invokes(call => capturedAlerts.AddRange(call.GetArgument<IEnumerable<AlertAggregate>>(0)!));

        var handler = CreateHandler(customThresholds);
        await handler.Handle(@event, ct);

        capturedAlerts.ShouldNotContain(a => a.Type == AlertType.HighTemperature);
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private SensorIngestedHandler CreateHandler(AlertThresholdOptions? thresholds = null)
    {
        var options = Options.Create(thresholds ?? _defaultThresholds);
        return new SensorIngestedHandler(
            NullLogger<SensorIngestedHandler>.Instance,
            options,
            _alertRepository,
            _unitOfWork,
            _hubNotifier);
    }

    private static EventContext<SensorIngestedIntegrationEvent> BuildEvent(
        Guid? sensorId = null,
        double? temperature = 25,
        double? soilMoisture = 45,
        double? batteryLevel = 80,
        double? humidity = 60,
        double? rainfall = 0)
    {
        var sid = sensorId ?? Guid.NewGuid();
        var evt = new SensorIngestedIntegrationEvent(
            SensorReadingId: Guid.NewGuid(),
            SensorId: sid,
            Time: DateTimeOffset.UtcNow,
            Temperature: temperature,
            Humidity: humidity,
            SoilMoisture: soilMoisture,
            Rainfall: rainfall,
            BatteryLevel: batteryLevel,
            OccurredOn: DateTimeOffset.UtcNow);

        return new EventContext<SensorIngestedIntegrationEvent>(
            eventData: evt,
            messageId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            aggregateId: sid,
            userId: null,
            isAuthenticated: false,
            correlationId: null,
            source: "test",
            eventType: nameof(SensorIngestedIntegrationEvent),
            aggregateType: "SensorReading",
            version: 1,
            metadata: null);
    }
}
