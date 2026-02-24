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

        var activeAlerts = await _alertReadStore.GetPendingAlertsAsync(
            pageNumber: 1,
            pageSize: 100,
            cancellationToken: cancellationToken);

        var plotAlerts = activeAlerts.Data
            .Where(a => sensorIds.Contains(a.SensorId))
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .ToList();

        foreach (var alert in plotAlerts)
        {
            var sensor = sensors.FirstOrDefault(s => s.Id == alert.SensorId);
            if (sensor == null)
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
