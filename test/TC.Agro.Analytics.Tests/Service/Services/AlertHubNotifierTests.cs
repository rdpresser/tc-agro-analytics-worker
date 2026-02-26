using FakeItEasy;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Domain.Snapshots;
using TC.Agro.Analytics.Service.Hubs;
using TC.Agro.Analytics.Service.Services;
using TC.Agro.SharedKernel.Infrastructure.Caching.Service;

namespace TC.Agro.Analytics.Tests.Service.Services;

public class AlertHubNotifierTests
{
    private readonly IHubContext<AlertHub, IAlertHubClient> _hubContext;
    private readonly ISensorSnapshotStore _snapshotStore;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AlertHubNotifier> _logger;
    private readonly AlertHubNotifier _notifier;

    public AlertHubNotifierTests()
    {
        _hubContext = A.Fake<IHubContext<AlertHub, IAlertHubClient>>();
        _snapshotStore = A.Fake<ISensorSnapshotStore>();
        _cacheService = A.Fake<ICacheService>();
        _logger = NullLogger<AlertHubNotifier>.Instance;
        _notifier = new AlertHubNotifier(_hubContext, _snapshotStore, _cacheService, _logger);
    }

    [Fact]
    public async Task NotifyAlertCreatedAsync_WithExistingSnapshot_ShouldNotThrow()
    {
        var sensorId = Guid.NewGuid();
        var snapshot = SensorSnapshot.Create(
            sensorId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor-001",
            "Plot 1",
            "Property 1");

        A.CallTo(_cacheService)
            .Where(call => call.Method.Name == "GetOrSetAsync"
                           && call.Arguments.Count >= 1
                           && Equals(call.Arguments[0], $"sensor:snapshot:{sensorId}"))
            .WithReturnType<ValueTask<SensorSnapshot?>>()
            .Returns(new ValueTask<SensorSnapshot?>(snapshot));

        var exception = await Record.ExceptionAsync(() =>
            _notifier.NotifyAlertCreatedAsync(
                Guid.NewGuid(),
                sensorId,
                "LowSoilMoisture",
                "High",
                "Soil moisture below threshold",
                25,
                30,
                DateTimeOffset.UtcNow));

        exception.ShouldBeNull();
    }

    [Fact]
    public async Task NotifyAlertAcknowledgedAsync_WithExistingSnapshot_ShouldNotThrow()
    {
        var sensorId = Guid.NewGuid();
        var snapshot = SensorSnapshot.Create(
            sensorId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor-001",
            "Plot 1",
            "Property 1");

        A.CallTo(_cacheService)
            .Where(call => call.Method.Name == "GetOrSetAsync"
                           && call.Arguments.Count >= 1
                           && Equals(call.Arguments[0], $"sensor:snapshot:{sensorId}"))
            .WithReturnType<ValueTask<SensorSnapshot?>>()
            .Returns(new ValueTask<SensorSnapshot?>(snapshot));

        var exception = await Record.ExceptionAsync(() =>
            _notifier.NotifyAlertAcknowledgedAsync(
                Guid.NewGuid(),
                sensorId,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));

        exception.ShouldBeNull();
    }

    [Fact]
    public async Task NotifyAlertResolvedAsync_WithExistingSnapshot_ShouldNotThrow()
    {
        var sensorId = Guid.NewGuid();
        var snapshot = SensorSnapshot.Create(
            sensorId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor-001",
            "Plot 1",
            "Property 1");

        A.CallTo(_cacheService)
            .Where(call => call.Method.Name == "GetOrSetAsync"
                           && call.Arguments.Count >= 1
                           && Equals(call.Arguments[0], $"sensor:snapshot:{sensorId}"))
            .WithReturnType<ValueTask<SensorSnapshot?>>()
            .Returns(new ValueTask<SensorSnapshot?>(snapshot));

        var exception = await Record.ExceptionAsync(() =>
            _notifier.NotifyAlertResolvedAsync(
                Guid.NewGuid(),
                sensorId,
                Guid.NewGuid(),
                "resolved",
                DateTimeOffset.UtcNow));

        exception.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullHubContext_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AlertHubNotifier(null!, _snapshotStore, _cacheService, _logger));
    }

    [Fact]
    public void Constructor_WithNullSnapshotStore_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AlertHubNotifier(_hubContext, null!, _cacheService, _logger));
    }

    [Fact]
    public void Constructor_WithNullCacheService_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AlertHubNotifier(_hubContext, _snapshotStore, null!, _logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() =>
            new AlertHubNotifier(_hubContext, _snapshotStore, _cacheService, null!));
    }
}
