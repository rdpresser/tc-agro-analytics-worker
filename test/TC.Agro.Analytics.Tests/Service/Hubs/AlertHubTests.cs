using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;
using TC.Agro.Analytics.Domain.Snapshots;
using TC.Agro.Analytics.Service.Hubs;

namespace TC.Agro.Analytics.Tests.Service.Hubs;

public class AlertHubTests
{
    private readonly IAlertReadStore _alertReadStore;
    private readonly ISensorSnapshotStore _snapshotStore;
    private readonly AlertHub _hub;
    private readonly IAlertHubClient _callerClient;
    private readonly IGroupManager _groups;
    private readonly HubCallerContext _hubCallerContext;

    public AlertHubTests()
    {
        _alertReadStore = A.Fake<IAlertReadStore>();
        _snapshotStore = A.Fake<ISensorSnapshotStore>();
        _callerClient = A.Fake<IAlertHubClient>();
        _hub = new AlertHub(_alertReadStore, _snapshotStore, NullLogger<AlertHub>.Instance);

        _hubCallerContext = A.Fake<HubCallerContext>();
        A.CallTo(() => _hubCallerContext.ConnectionId).Returns("test-connection-id");
        _hub.Context = _hubCallerContext;

        var clients = A.Fake<IHubCallerClients<IAlertHubClient>>();
        A.CallTo(() => clients.Caller).Returns(_callerClient);
        _hub.Clients = clients;

        _groups = A.Fake<IGroupManager>();
        _hub.Groups = _groups;

        A.CallTo(() => _snapshotStore.GetByOwnerIdAsync(A<Guid>._, A<CancellationToken>._))
            .Returns(Array.Empty<SensorSnapshot>());

        A.CallTo(() => _snapshotStore.GetByPlotIdAsync(A<Guid>._, A<CancellationToken>._))
            .Returns(Array.Empty<SensorSnapshot>());

        A.CallTo(() => _alertReadStore.GetPendingAlertsBySensorIdsAsync(A<IEnumerable<Guid>>._, 20, A<CancellationToken>._))
            .Returns(Array.Empty<PendingAlertResponse>());
    }

    [Fact]
    public async Task JoinPlotGroup_WithValidPlotId_ShouldAddGroupAndSendRecentAlerts()
    {
        var ownerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();

        var snapshot = SensorSnapshot.Create(
            sensorId,
            ownerId,
            propertyId,
            plotId,
            "Sensor-001",
            "Plot 1",
            "Property 1");

        var alert = new PendingAlertResponse(
            Guid.NewGuid(),
            sensorId,
            "LowSoilMoisture",
            "Soil moisture below threshold",
            "Pending",
            "High",
            25,
            30,
            DateTimeOffset.UtcNow,
            null,
            null);

        A.CallTo(() => _snapshotStore.GetByPlotIdAsync(plotId, A<CancellationToken>._))
            .Returns(new[] { snapshot });

        A.CallTo(() => _alertReadStore.GetPendingAlertsBySensorIdsAsync(
                A<IEnumerable<Guid>>.That.Matches(ids => ids.Contains(sensorId)),
                20,
                A<CancellationToken>._))
            .Returns(new[] { alert });

        await _hub.JoinPlotGroup(plotId.ToString());

        A.CallTo(() => _groups.AddToGroupAsync(
                "test-connection-id",
                $"plot:{plotId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _callerClient.AlertCreated(
                A<AlertCreatedNotification>.That.Matches(n =>
                    n.SensorId == sensorId &&
                    n.PlotId == plotId)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task JoinPlotGroup_WithInvalidGuid_ShouldThrowHubException()
    {
        await Should.ThrowAsync<HubException>(() => _hub.JoinPlotGroup("invalid-plot"));
    }

    [Fact]
    public async Task LeavePlotGroup_WithValidPlotId_ShouldRemoveFromGroup()
    {
        var plotId = Guid.NewGuid();

        await _hub.LeavePlotGroup(plotId.ToString());

        A.CallTo(() => _groups.RemoveFromGroupAsync(
                "test-connection-id",
                $"plot:{plotId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task JoinOwnerGroup_WithAdminRole_ShouldJoinOwnerAndPlotGroups()
    {
        var ownerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();

        SetUserContext(new[] { new Claim(ClaimTypes.Role, "Admin") });

        var snapshot = SensorSnapshot.Create(
            sensorId,
            ownerId,
            propertyId,
            plotId,
            "Sensor-001",
            "Plot 1",
            "Property 1");

        A.CallTo(() => _snapshotStore.GetByOwnerIdAsync(ownerId, A<CancellationToken>._))
            .Returns(new[] { snapshot });

        await _hub.JoinOwnerGroup(ownerId.ToString());

        A.CallTo(() => _groups.AddToGroupAsync(
                "test-connection-id",
                $"owner:{ownerId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _groups.AddToGroupAsync(
                "test-connection-id",
                $"plot:{plotId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task JoinOwnerGroup_WithProducerRole_ShouldUseOwnerFromClaims()
    {
        var claimOwnerId = Guid.NewGuid();

        SetUserContext(new[]
        {
            new Claim(ClaimTypes.Role, "Producer"),
            new Claim(ClaimTypes.NameIdentifier, claimOwnerId.ToString())
        });

        await _hub.JoinOwnerGroup(Guid.NewGuid().ToString());

        A.CallTo(() => _groups.AddToGroupAsync(
                "test-connection-id",
                $"owner:{claimOwnerId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task JoinOwnerGroup_WithProducerRoleAndOidClaim_ShouldUseOwnerFromClaims()
    {
        var claimOwnerId = Guid.NewGuid();

        SetUserContext(new[]
        {
            new Claim(ClaimTypes.Role, "Producer"),
            new Claim("oid", claimOwnerId.ToString())
        });

        await _hub.JoinOwnerGroup(Guid.NewGuid().ToString());

        A.CallTo(() => _groups.AddToGroupAsync(
                "test-connection-id",
                $"owner:{claimOwnerId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task LeaveOwnerGroup_WithAdminRole_ShouldRemoveOwnerAndPlotGroups()
    {
        var ownerId = Guid.NewGuid();
        var plotId = Guid.NewGuid();

        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            ownerId,
            Guid.NewGuid(),
            plotId,
            "Sensor-001",
            "Plot 1",
            "Property 1");

        A.CallTo(() => _snapshotStore.GetByOwnerIdAsync(ownerId, A<CancellationToken>._))
            .Returns(new[] { snapshot });

        SetUserContext(new[] { new Claim(ClaimTypes.Role, "Admin") });

        await _hub.LeaveOwnerGroup(ownerId.ToString());

        A.CallTo(() => _groups.RemoveFromGroupAsync(
                "test-connection-id",
                $"owner:{ownerId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _groups.RemoveFromGroupAsync(
                "test-connection-id",
                $"plot:{plotId}",
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task JoinOwnerGroup_WithAdminRoleAndInvalidOwnerId_ShouldThrowHubException()
    {
        SetUserContext(new[] { new Claim(ClaimTypes.Role, "Admin") });

        await Should.ThrowAsync<HubException>(() => _hub.JoinOwnerGroup("not-a-guid"));
    }

    private void SetUserContext(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "test-auth", ClaimTypes.NameIdentifier, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        A.CallTo(() => _hubCallerContext.User).Returns(principal);
    }
}
