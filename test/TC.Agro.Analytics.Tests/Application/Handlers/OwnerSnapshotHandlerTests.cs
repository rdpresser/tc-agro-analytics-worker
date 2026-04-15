using FakeItEasy;
using Microsoft.Extensions.Logging;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.MessageBrokerHandlers;
using TC.Agro.Analytics.Domain.Snapshots;
using TC.Agro.Contracts.Events.Farm;
using TC.Agro.Contracts.Events.Identity;
using TC.Agro.SharedKernel.Application.Ports;
using TC.Agro.SharedKernel.Infrastructure.Messaging;

namespace TC.Agro.Analytics.Tests.Application.Handlers;

public sealed class OwnerSnapshotHandlerTests
{
    private readonly IOwnerSnapshotStore _store = A.Fake<IOwnerSnapshotStore>();
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    // ──────────────────────────────────────────
    // UserCreated — guard
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserCreated_WhenEventIsNull_ShouldThrow()
    {
        var handler = CreateHandler();

        await Should.ThrowAsync<ArgumentNullException>(
            () => handler.HandleAsync((EventContext<UserCreatedIntegrationEvent>)null!, CancellationToken.None));
    }

    // ──────────────────────────────────────────
    // UserCreated — non-producer role is ignored
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserCreated_WhenRoleIsNotProducer_ShouldNotCreateSnapshot()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildUserCreatedEvent(role: "Admin");

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        A.CallTo(() => _store.AddAsync(A<OwnerSnapshot>._, ct)).MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // UserCreated — snapshot already exists
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserCreated_WhenSnapshotAlreadyExists_ShouldNotCreateDuplicate()
    {
        var ct = TestContext.Current.CancellationToken;
        var ownerId = Guid.NewGuid();
        var @event = BuildUserCreatedEvent(ownerId: ownerId, role: "Producer");

        var existingSnapshot = OwnerSnapshot.Create(ownerId, "Existing Name", "existing@test.com");

        A.CallTo(() => _store.GetByIdAsync(ownerId, ct)).Returns(existingSnapshot);

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        A.CallTo(() => _store.AddAsync(A<OwnerSnapshot>._, ct)).MustNotHaveHappened();
        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // UserCreated — success
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserCreated_WithProducerRoleAndNoExistingSnapshot_ShouldCreateAndPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var ownerId = Guid.NewGuid();
        var @event = BuildUserCreatedEvent(ownerId: ownerId, role: "Producer", name: "João Silva", email: "joao@farm.com");

        A.CallTo(() => _store.GetByIdAsync(ownerId, ct)).Returns(Task.FromResult<OwnerSnapshot?>(null));

        OwnerSnapshot? captured = null;
        A.CallTo(() => _store.AddAsync(A<OwnerSnapshot>._, ct))
            .Invokes(call => captured = call.GetArgument<OwnerSnapshot>(0));

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        captured.ShouldNotBeNull();
        captured!.Id.ShouldBe(ownerId);
        captured.Name.ShouldBe("João Silva");
        captured.Email.ShouldBe("joao@farm.com");
        captured.IsActive.ShouldBeTrue();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // UserUpdated — snapshot not found is silently ignored
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserUpdated_WhenSnapshotNotFound_ShouldNotPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildUserUpdatedEvent();

        A.CallTo(() => _store.GetByIdAsync(A<Guid>._, ct)).Returns(Task.FromResult<OwnerSnapshot?>(null));

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // UserUpdated — updates name and email
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserUpdated_WhenSnapshotExists_ShouldUpdateAndPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var ownerId = Guid.NewGuid();
        var snapshot = OwnerSnapshot.Create(ownerId, "Old Name", "old@test.com");
        var @event = BuildUserUpdatedEvent(ownerId: ownerId, name: "New Name", email: "new@test.com");

        A.CallTo(() => _store.GetByIdAsync(ownerId, ct)).Returns(snapshot);

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        snapshot.Name.ShouldBe("New Name");
        snapshot.Email.ShouldBe("new@test.com");

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // UserDeactivated — snapshot not found is silently ignored
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserDeactivated_WhenSnapshotNotFound_ShouldNotPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var @event = BuildUserDeactivatedEvent();

        A.CallTo(() => _store.GetByIdAsync(A<Guid>._, ct)).Returns(Task.FromResult<OwnerSnapshot?>(null));

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustNotHaveHappened();
    }

    // ──────────────────────────────────────────
    // UserDeactivated — marks snapshot as inactive
    // ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_UserDeactivated_WhenSnapshotExists_ShouldMarkInactiveAndPersist()
    {
        var ct = TestContext.Current.CancellationToken;
        var ownerId = Guid.NewGuid();
        var snapshot = OwnerSnapshot.Create(ownerId, "João", "joao@farm.com");
        var @event = BuildUserDeactivatedEvent(ownerId: ownerId);

        A.CallTo(() => _store.GetByIdAsync(ownerId, ct)).Returns(snapshot);

        var handler = CreateHandler();
        await handler.HandleAsync(@event, ct);

        snapshot.IsActive.ShouldBeFalse();

        A.CallTo(() => _unitOfWork.SaveChangesAsync(ct)).MustHaveHappenedOnceExactly();
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private OwnerSnapshotHandler CreateHandler() => new(_store, _unitOfWork);

    private static EventContext<UserCreatedIntegrationEvent> BuildUserCreatedEvent(
        Guid? ownerId = null,
        string role = "Producer",
        string name = "Test Owner",
        string email = "owner@test.com")
    {
        var id = ownerId ?? Guid.NewGuid();
        var evt = new UserCreatedIntegrationEvent(
            OwnerId: id,
            Name: name,
            Email: email,
            Username: "testowner",
            Role: role,
            OccurredOn: DateTimeOffset.UtcNow);

        return new EventContext<UserCreatedIntegrationEvent>(
            eventData: evt,
            messageId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            aggregateId: id,
            userId: null,
            isAuthenticated: false,
            correlationId: null,
            source: "test",
            eventType: nameof(UserCreatedIntegrationEvent),
            aggregateType: "User",
            version: 1,
            metadata: null);
    }

    private static EventContext<UserUpdatedIntegrationEvent> BuildUserUpdatedEvent(
        Guid? ownerId = null,
        string name = "Updated Name",
        string email = "updated@test.com")
    {
        var id = ownerId ?? Guid.NewGuid();
        var evt = new UserUpdatedIntegrationEvent(
            OwnerId: id,
            Name: name,
            Email: email,
            Username: "updatedowner",
            OccurredOn: DateTimeOffset.UtcNow);

        return new EventContext<UserUpdatedIntegrationEvent>(
            eventData: evt,
            messageId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            aggregateId: id,
            userId: null,
            isAuthenticated: false,
            correlationId: null,
            source: "test",
            eventType: nameof(UserUpdatedIntegrationEvent),
            aggregateType: "User",
            version: 1,
            metadata: null);
    }

    private static EventContext<UserDeactivatedIntegrationEvent> BuildUserDeactivatedEvent(Guid? ownerId = null)
    {
        var id = ownerId ?? Guid.NewGuid();

        // Find the UserDeactivatedIntegrationEvent shape from contracts
        var evt = new UserDeactivatedIntegrationEvent(
            OwnerId: id,
            OccurredOn: DateTimeOffset.UtcNow);

        return new EventContext<UserDeactivatedIntegrationEvent>(
            eventData: evt,
            messageId: Guid.NewGuid(),
            occurredAt: DateTimeOffset.UtcNow,
            aggregateId: id,
            userId: null,
            isAuthenticated: false,
            correlationId: null,
            source: "test",
            eventType: nameof(UserDeactivatedIntegrationEvent),
            aggregateType: "User",
            version: 1,
            metadata: null);
    }
}
