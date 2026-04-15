using FakeItEasy;
using Microsoft.Extensions.Logging;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.MessageBrokerHandlers;
using TC.Agro.Analytics.Domain.Snapshots;
using TC.Agro.Contracts.Events.Farm;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.Messaging;

namespace TC.Agro.Analytics.Tests.Application.Handlers;

public sealed class SensorSnapshotHandlerTests
{
    private readonly ISensorSnapshotStore _store = A.Fake<ISensorSnapshotStore>();
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly ILogger<SensorSnapshotHandler> _logger = A.Fake<ILogger<SensorSnapshotHandler>>();

    // ──────────────────────────────────────────
    // Guard clauses
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SensorRegistered_WhenEventIsNull_ShouldThrow()
    {
        var handler = CreateHandler();

        await Should.ThrowAsync<ArgumentNullException>(
            () => handler.HandleAsync((EventContext<SensorRegisteredIntegrationEvent>)null!, CancellationToken.None));
    }

    // ──────────────────────────────────────────
    // SensorRegistered — creates new snapshot
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SensorRegistered_ShouldCreateSnapshotAndPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var @event = BuildSensorRegisteredEvent(sensorId, ownerId, propertyId, plotId, label: "Sensor-A");

        SensorSnapshot? captured = null;
        A.CallTo(() => _store.AddAsync(A<SensorSnapshot>._, ct))
            .Invokes(call => captured = call.GetArgument<SensorSnapshot>(0));

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        captured.ShouldNotBeNull();
        captured!.Id.ShouldBe(sensorId);
        captured.OwnerId.ShouldBe(ownerId);
        captured.PropertyId.ShouldBe(propertyId);
        captured.PlotId.ShouldBe(plotId);
        captured.Label.ShouldBe("Sensor-A");
        captured.IsActive.ShouldBeTrue();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task HandleAsync_SensorRegistered_WithNullLabel_ShouldUseDefaultLabel()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildSensorRegisteredEvent(label: null);

        SensorSnapshot? captured = null;
        A.CallTo(() => _store.AddAsync(A<SensorSnapshot>._, ct))
            .Invokes(call => captured = call.GetArgument<SensorSnapshot>(0));

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        captured.ShouldNotBeNull();
        captured!.Label.ShouldBe("Unnamed Sensor");
    }

    [Fact]
    public async Task HandleAsync_SensorRegistered_WithEmptyLabel_ShouldUseDefaultLabel()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildSensorRegisteredEvent(label: "   ");

        SensorSnapshot? captured = null;
        A.CallTo(() => _store.AddAsync(A<SensorSnapshot>._, ct))
            .Invokes(call => captured = call.GetArgument<SensorSnapshot>(0));

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        captured!.Label.ShouldBe("Unnamed Sensor");
    }

    // ──────────────────────────────────────────
    // SensorOperationalStatusChanged — existing snapshot updated
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SensorStatusChanged_WhenSnapshotExists_ShouldUpdateAndPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();

        var existingSnapshot = SensorSnapshot.Create(
            sensorId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "OldLabel", "OldPlot", "OldProperty", status: "Active");

        var @event = BuildSensorStatusChangedEvent(sensorId, newStatus: "Maintenance", reason: "Annual check", label: "NewLabel");

        A.CallTo(() => _store.GetByIdAsync(sensorId, ct)).Returns(existingSnapshot);

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        existingSnapshot.Status.ShouldBe("Maintenance");
        existingSnapshot.StatusChangeReason.ShouldBe("Annual check");
        existingSnapshot.Label.ShouldBe("NewLabel");

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // SensorOperationalStatusChanged — snapshot missing, creates defensively
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SensorStatusChanged_WhenSnapshotNotFound_ShouldCreateDefensively()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();

        A.CallTo(() => _store.GetByIdAsync(sensorId, ct)).Returns(Task.FromResult<SensorSnapshot?>(null));

        var @event = BuildSensorStatusChangedEvent(sensorId, newStatus: "Active", reason: null, label: "NewSensor");

        SensorSnapshot? captured = null;
        A.CallTo(() => _store.AddAsync(A<SensorSnapshot>._, ct))
            .Invokes(call => captured = call.GetArgument<SensorSnapshot>(0));

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        // AddAsync called defensively
        A.CallTo(() => _store.AddAsync(A<SensorSnapshot>._, ct)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // SensorDeactivated — marks inactive (soft-delete)
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_SensorDeactivated_ShouldCallDeleteAndPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var sensorId = Guid.NewGuid();
        var @event = BuildSensorDeactivatedEvent(sensorId, reason: "End of life");

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        A.CallTo(() => _store.DeleteAsync(sensorId, ct)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private SensorSnapshotHandler CreateHandler() => new(_store, _unitOfWork, _logger);

    private static EventContext<SensorRegisteredIntegrationEvent> BuildSensorRegisteredEvent(
        Guid? sensorId = null,
        Guid? ownerId = null,
        Guid? propertyId = null,
        Guid? plotId = null,
        string? label = "Sensor-001",
        string status = "Active")
    {
        var sid = sensorId ?? Guid.NewGuid();
        var evt = new SensorRegisteredIntegrationEvent(
            SensorId: sid,
            OwnerId: ownerId ?? Guid.NewGuid(),
            PropertyId: propertyId ?? Guid.NewGuid(),
            PlotId: plotId ?? Guid.NewGuid(),
            Label: label,
            PropertyName: "Test Property",
            PlotName: "Test Plot",
            Type: "Temperature",
            Status: status,
            OccurredOn: DateTimeOffset.UtcNow);

        return new EventContext<SensorRegisteredIntegrationEvent>(
            eventData: evt,
            messageId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            aggregateId: sid,
            userId: null,
            isAuthenticated: false,
            correlationId: null,
            source: "test",
            eventType: nameof(SensorRegisteredIntegrationEvent),
            aggregateType: "Sensor",
            version: 1,
            metadata: null);
    }

    private static EventContext<SensorOperationalStatusChangedIntegrationEvent> BuildSensorStatusChangedEvent(
        Guid sensorId,
        string newStatus,
        string? reason,
        string? label = "Sensor-001")
    {
        var evt = new SensorOperationalStatusChangedIntegrationEvent(
            SensorId: sensorId,
            OwnerId: Guid.NewGuid(),
            PropertyId: Guid.NewGuid(),
            PlotId: Guid.NewGuid(),
            Label: label,
            PropertyName: "Property",
            PlotName: "Plot",
            Status: newStatus,
            Reason: reason,
            OccurredOn: DateTimeOffset.UtcNow);

        return new EventContext<SensorOperationalStatusChangedIntegrationEvent>(
            eventData: evt,
            messageId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            aggregateId: sensorId,
            userId: null,
            isAuthenticated: false,
            correlationId: null,
            source: "test",
            eventType: nameof(SensorOperationalStatusChangedIntegrationEvent),
            aggregateType: "Sensor",
            version: 1,
            metadata: null);
    }

    private static EventContext<SensorDeactivatedIntegrationEvent> BuildSensorDeactivatedEvent(
        Guid sensorId,
        string reason = "Decommissioned")
    {
        var evt = new SensorDeactivatedIntegrationEvent(
            SensorId: sensorId,
            PlotId: Guid.NewGuid(),
            PropertyId: Guid.NewGuid(),
            Reason: reason,
            DeactivatedByUserId: Guid.NewGuid(),
            OccurredOn: DateTimeOffset.UtcNow);

        return new EventContext<SensorDeactivatedIntegrationEvent>(
            eventData: evt,
            messageId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            aggregateId: sensorId,
            userId: null,
            isAuthenticated: false,
            correlationId: null,
            source: "test",
            eventType: nameof(SensorDeactivatedIntegrationEvent),
            aggregateType: "Sensor",
            version: 1,
            metadata: null);
    }
}
