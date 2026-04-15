using FakeItEasy;
using Microsoft.Extensions.Logging;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.UseCases.Alerts.AcknowledgeAlert;
using TC.Agro.Analytics.Application.UseCases.Alerts.ResolveAlert;
using TC.Agro.Analytics.Tests.TestHelpers;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;

namespace TC.Agro.Analytics.Tests.Application.Handlers;

public sealed class AcknowledgeAlertCommandHandlerTests
{
    private readonly IAlertAggregateRepository _repository = A.Fake<IAlertAggregateRepository>();
    private readonly IUserContext _userContext = A.Fake<IUserContext>();
    private readonly ITransactionalOutbox _outbox = A.Fake<ITransactionalOutbox>();
    private readonly IAlertHubNotifier _hubNotifier = A.Fake<IAlertHubNotifier>();
    private readonly ILogger<AcknowledgeAlertCommandHandler> _logger =
        A.Fake<ILogger<AcknowledgeAlertCommandHandler>>();

    public AcknowledgeAlertCommandHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();

        A.CallTo(() => _userContext.Id).Returns(Guid.NewGuid());

        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(1));
    }

    // ──────────────────────────────────────────
    // Not found
    // ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAlertNotFound_ShouldReturnNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var command = new AcknowledgeAlertCommand(Guid.NewGuid());

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(null));

        var handler = CreateHandler();
        var result = await handler.ExecuteAsync(command, ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Errors.ShouldContain(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase));

        A.CallTo(() => _hubNotifier.NotifyAlertAcknowledgedAsync(
            A<Guid>._, A<Guid>._, A<Guid>._, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // Invalid state — already acknowledged
    // ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAlertAlreadyAcknowledged_ShouldReturnInvalid()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        A.CallTo(() => _userContext.Id).Returns(userId);

        var alert = CreatePendingAlert();
        alert.Acknowledge(userId); // acknowledge first time

        var command = new AcknowledgeAlertCommand(alert.Id);

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(alert));

        var handler = CreateHandler();
        var result = await handler.ExecuteAsync(command, ct);

        result.Status.ShouldBe(ResultStatus.Invalid);

        A.CallTo(() => _hubNotifier.NotifyAlertAcknowledgedAsync(
            A<Guid>._, A<Guid>._, A<Guid>._, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // Success
    // ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithPendingAlert_ShouldAcknowledgeAndSendSignalRNotification()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        A.CallTo(() => _userContext.Id).Returns(userId);

        var alert = CreatePendingAlert();
        var command = new AcknowledgeAlertCommand(alert.Id);

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(alert));

        var handler = CreateHandler();
        var result = await handler.ExecuteAsync(command, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(alert.Id);
        result.Value.Status.ShouldBe("Acknowledged");
        result.Value.AcknowledgedBy.ShouldBe(userId);

        A.CallTo(() => _hubNotifier.NotifyAlertAcknowledgedAsync(
            alert.Id, alert.SensorId, userId, A<DateTimeOffset>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_OnSuccess_ShouldCallOutboxSaveChanges()
    {
        var ct = TestContext.Current.CancellationToken;
        var alert = CreatePendingAlert();
        var command = new AcknowledgeAlertCommand(alert.Id);

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(alert));

        var handler = CreateHandler();
        await handler.ExecuteAsync(command, ct);

        A.CallTo(() => _outbox.SaveChangesAsync(ct))
            .MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private AcknowledgeAlertCommandHandler CreateHandler()
        => new(_repository, _userContext, _outbox, _hubNotifier, _logger);

    private static AlertAggregate CreatePendingAlert()
    {
        var result = AlertAggregate.Create(
            sensorId: Guid.NewGuid(),
            type: AlertType.HighTemperature,
            severity: AlertSeverity.High,
            message: "High temperature detected: 40°C",
            value: 40,
            threshold: 35);

        result.IsSuccess.ShouldBeTrue();
        return result.Value;
    }
}

public sealed class ResolveAlertCommandHandlerTests
{
    private readonly IAlertAggregateRepository _repository = A.Fake<IAlertAggregateRepository>();
    private readonly IUserContext _userContext = A.Fake<IUserContext>();
    private readonly ITransactionalOutbox _outbox = A.Fake<ITransactionalOutbox>();
    private readonly IAlertHubNotifier _hubNotifier = A.Fake<IAlertHubNotifier>();
    private readonly ILogger<ResolveAlertCommandHandler> _logger =
        A.Fake<ILogger<ResolveAlertCommandHandler>>();

    public ResolveAlertCommandHandlerTests()
    {
        FastEndpointsTestBootstrap.EnsureInitialized();

        A.CallTo(() => _userContext.Id).Returns(Guid.NewGuid());

        A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(1));
    }

    // ──────────────────────────────────────────
    // Not found
    // ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAlertNotFound_ShouldReturnNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var command = new ResolveAlertCommand(Guid.NewGuid(), "Technician fixed irrigation");

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(null));

        var handler = CreateHandler();
        var result = await handler.ExecuteAsync(command, ct);

        result.Status.ShouldBe(ResultStatus.NotFound);

        A.CallTo(() => _hubNotifier.NotifyAlertResolvedAsync(
            A<Guid>._, A<Guid>._, A<Guid>._, A<string>._, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // Invalid state — already resolved
    // ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAlertAlreadyResolved_ShouldReturnInvalid()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        A.CallTo(() => _userContext.Id).Returns(userId);

        var alert = CreatePendingAlert();
        alert.Acknowledge(userId);
        alert.Resolve(userId, "First resolution");

        var command = new ResolveAlertCommand(alert.Id, "Second attempt");

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(alert));

        var handler = CreateHandler();
        var result = await handler.ExecuteAsync(command, ct);

        result.Status.ShouldBe(ResultStatus.Invalid);

        A.CallTo(() => _hubNotifier.NotifyAlertResolvedAsync(
            A<Guid>._, A<Guid>._, A<Guid>._, A<string>._, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // Success — from Pending (direct resolve)
    // ──────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithPendingAlert_ShouldResolveAndSendSignalRNotification()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        A.CallTo(() => _userContext.Id).Returns(userId);

        var alert = CreatePendingAlert();
        var notes = "Technician inspected the sensor and restored normal conditions.";
        var command = new ResolveAlertCommand(alert.Id, notes);

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(alert));

        var handler = CreateHandler();
        var result = await handler.ExecuteAsync(command, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Status.ShouldBe("Resolved");
        result.Value.ResolvedBy.ShouldBe(userId);
        result.Value.ResolutionNotes.ShouldBe(notes);

        A.CallTo(() => _hubNotifier.NotifyAlertResolvedAsync(
            alert.Id, alert.SensorId, userId, notes, A<DateTimeOffset>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullResolutionNotes_ShouldSucceed()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        A.CallTo(() => _userContext.Id).Returns(userId);

        var alert = CreatePendingAlert();
        var command = new ResolveAlertCommand(alert.Id, null);

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(alert));

        var handler = CreateHandler();
        var result = await handler.ExecuteAsync(command, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ResolutionNotes.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_OnSuccess_ShouldCallOutboxSaveChangesOnce()
    {
        var ct = TestContext.Current.CancellationToken;
        var alert = CreatePendingAlert();
        var command = new ResolveAlertCommand(alert.Id, "Fixed");

        A.CallTo(() => _repository.GetByIdAsync(command.AlertId, ct))
            .Returns(Task.FromResult<AlertAggregate?>(alert));

        var handler = CreateHandler();
        await handler.ExecuteAsync(command, ct);

        A.CallTo(() => _outbox.SaveChangesAsync(ct))
            .MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private ResolveAlertCommandHandler CreateHandler()
        => new(_repository, _userContext, _outbox, _hubNotifier, _logger);

    private static AlertAggregate CreatePendingAlert()
    {
        var result = AlertAggregate.Create(
            sensorId: Guid.NewGuid(),
            type: AlertType.LowSoilMoisture,
            severity: AlertSeverity.Medium,
            message: "Low soil moisture detected: 15%",
            value: 15,
            threshold: 30);

        result.IsSuccess.ShouldBeTrue();
        return result.Value;
    }
}
