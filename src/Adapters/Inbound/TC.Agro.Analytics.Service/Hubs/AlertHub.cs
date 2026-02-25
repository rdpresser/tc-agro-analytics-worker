namespace TC.Agro.Analytics.Service.Hubs;

[Authorize(Roles = "Admin,Producer")]
public sealed class AlertHub : Hub<IAlertHubClient>
{
    private readonly IAlertReadStore _alertReadStore;
    private readonly ISensorSnapshotStore _snapshotStore;

    public AlertHub(
        IAlertReadStore alertReadStore,
        ISensorSnapshotStore snapshotStore
        )
    {
        _alertReadStore = alertReadStore ?? throw new ArgumentNullException(nameof(alertReadStore));
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
    }

    public async Task JoinPlotGroup(string plotId)
    {
        if (!Guid.TryParse(plotId, out var parsedPlotId) || parsedPlotId == Guid.Empty)
            throw new HubException("Invalid plotId. Must be a valid non-empty GUID.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"plot:{parsedPlotId}");

        await SendRecentAlertsForPlotAsync(parsedPlotId, Context.ConnectionAborted).ConfigureAwait(false);
    }

    public async Task LeavePlotGroup(string plotId)
    {
        if (!Guid.TryParse(plotId, out var parsedPlotId) || parsedPlotId == Guid.Empty)
            throw new HubException("Invalid plotId. Must be a valid non-empty GUID.");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"plot:{parsedPlotId}").ConfigureAwait(false);
    }

    private async Task SendRecentAlertsForPlotAsync(Guid plotId, CancellationToken cancellationToken)
    {
        var sensors = await _snapshotStore.GetByPlotIdAsync(plotId, cancellationToken);
        if (!sensors.Any())
        {
            return;
        }

        var sensorIds = sensors.Select(s => s.Id).ToList();

        // Get pending alerts filtered by sensorIds at database level (performance optimization)
        var plotAlerts = await _alertReadStore.GetPendingAlertsBySensorIdsAsync(
            sensorIds,
            limit: 20,
            cancellationToken: cancellationToken);

        // Create dictionary for O(1) sensor lookup (performance optimization)
        var sensorLookup = sensors.ToDictionary(s => s.Id);

        foreach (var alert in plotAlerts)
        {
            // O(1) lookup instead of O(m) FirstOrDefault
            if (!sensorLookup.TryGetValue(alert.SensorId, out var sensor))
                continue;

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
