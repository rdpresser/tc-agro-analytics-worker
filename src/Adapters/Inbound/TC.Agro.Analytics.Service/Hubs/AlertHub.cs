using System.Security.Claims;

namespace TC.Agro.Analytics.Service.Hubs;

[Authorize(Roles = "Admin,Producer")]
public sealed class AlertHub : Hub<IAlertHubClient>
{
    private readonly IAlertReadStore _alertReadStore;
    private readonly ISensorSnapshotStore _snapshotStore;

    public AlertHub(
        IAlertReadStore alertReadStore,
        ISensorSnapshotStore snapshotStore)
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
    }

    public async Task JoinPlotGroup(string plotId)
    {
        if (!Guid.TryParse(plotId, out var parsedPlotId) || parsedPlotId == Guid.Empty)
            throw new HubException("Invalid plotId. Must be a valid non-empty GUID.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"plot:{parsedPlotId}").ConfigureAwait(false);
        await SendRecentAlertsForPlotAsync(parsedPlotId, Context.ConnectionAborted).ConfigureAwait(false);
    }

    public Task LeavePlotGroup(string plotId)
    {
        if (!Guid.TryParse(plotId, out var parsedPlotId) || parsedPlotId == Guid.Empty)
            throw new HubException("Invalid plotId. Must be a valid non-empty GUID.");

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"plot:{parsedPlotId}");
    }

    public async Task JoinOwnerGroup(string? ownerId = null)
    {
        var targetOwnerId = ResolveOwnerScope(ownerId);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"owner:{targetOwnerId}").ConfigureAwait(false);

        var ownerSnapshots = await _snapshotStore
            .GetByOwnerIdAsync(targetOwnerId, Context.ConnectionAborted)
            .ConfigureAwait(false);

        foreach (var plotId in ownerSnapshots.Select(snapshot => snapshot.PlotId).Distinct())
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"plot:{plotId}").ConfigureAwait(false);
        }

        await SendRecentAlertsForOwnerAsync(ownerSnapshots, Context.ConnectionAborted).ConfigureAwait(false);
    }

    public async Task LeaveOwnerGroup(string? ownerId = null)
    {
        var targetOwnerId = ResolveOwnerScope(ownerId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"owner:{targetOwnerId}").ConfigureAwait(false);

        var ownerSnapshots = await _snapshotStore
            .GetByOwnerIdAsync(targetOwnerId, Context.ConnectionAborted)
            .ConfigureAwait(false);

        foreach (var plotId in ownerSnapshots.Select(snapshot => snapshot.PlotId).Distinct())
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"plot:{plotId}").ConfigureAwait(false);
        }
    }

    private Guid ResolveOwnerScope(string? ownerId)
    {
        if (Context.User?.IsInRole("Admin") == true)
        {
            if (!Guid.TryParse(ownerId, out var adminTargetOwnerId) || adminTargetOwnerId == Guid.Empty)
                throw new HubException("Admin must provide a valid non-empty ownerId.");

            return adminTargetOwnerId;
        }

        var claimValue =
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        if (!Guid.TryParse(claimValue, out var currentOwnerId) || currentOwnerId == Guid.Empty)
            throw new HubException("Unable to resolve owner scope for current user.");

        return currentOwnerId;
    }

    private async Task SendRecentAlertsForPlotAsync(Guid plotId, CancellationToken cancellationToken)
    {
        var sensors = await _snapshotStore.GetByPlotIdAsync(plotId, cancellationToken).ConfigureAwait(false);
        if (!sensors.Any())
        {
            return;
        }

        var sensorIds = sensors.Select(s => s.Id).ToList();

        var plotAlerts = await _alertReadStore.GetPendingAlertsBySensorIdsAsync(
            sensorIds,
            limit: 20,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var sensorLookup = sensors.ToDictionary(s => s.Id);

        foreach (var alert in plotAlerts)
        {
            if (!sensorLookup.TryGetValue(alert.SensorId, out var sensor))
            {
                continue;
            }

            var notification = new AlertCreatedNotification(
                alert.Id,
                alert.SensorId,
                sensor.Label,
                sensor.PlotId,
                sensor.PlotName,
                sensor.PropertyName,
                alert.AlertType,
                alert.Severity,
                alert.Message,
                alert.Value ?? 0,
                alert.Threshold ?? 0,
                Metadata: null,
                alert.CreatedAt);

            await Clients.Caller.AlertCreated(notification).ConfigureAwait(false);
        }
    }

    private async Task SendRecentAlertsForOwnerAsync(
        IReadOnlyList<SensorSnapshot> ownerSnapshots,
        CancellationToken cancellationToken)
    {
        if (ownerSnapshots.Count == 0)
        {
            return;
        }

        var sensorLookup = ownerSnapshots.ToDictionary(snapshot => snapshot.Id);
        var sensorIds = sensorLookup.Keys.ToList();

        var ownerAlerts = await _alertReadStore.GetPendingAlertsBySensorIdsAsync(
            sensorIds,
            limit: 20,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var alert in ownerAlerts)
        {
            if (!sensorLookup.TryGetValue(alert.SensorId, out var sensor))
            {
                continue;
            }

            var notification = new AlertCreatedNotification(
                alert.Id,
                alert.SensorId,
                sensor.Label,
                sensor.PlotId,
                sensor.PlotName,
                sensor.PropertyName,
                alert.AlertType,
                alert.Severity,
                alert.Message,
                alert.Value ?? 0,
                alert.Threshold ?? 0,
                Metadata: null,
                alert.CreatedAt);

            await Clients.Caller.AlertCreated(notification).ConfigureAwait(false);
        }
    }
}
