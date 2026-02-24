namespace TC.Agro.Analytics.Service.Services;

internal sealed class AlertHubNotifier : IAlertHubNotifier
{
    private static readonly TimeSpan PlotIdCacheDuration = TimeSpan.FromMinutes(10);

    private readonly IHubContext<AlertHub, IAlertHubClient> _hubContext;
    private readonly ISensorSnapshotStore _snapshotStore;
    private readonly IFusionCache _cache;
    private readonly ILogger<AlertHubNotifier> _logger;

    public AlertHubNotifier(
        IHubContext<AlertHub, IAlertHubClient> hubContext,
        ISensorSnapshotStore snapshotStore,
        IFusionCache cache,
        ILogger<AlertHubNotifier> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task NotifyAlertCreatedAsync(
        Guid alertId,
        Guid sensorId,
        string alertType,
        string severity,
        string message,
        double value,
        double threshold,
        DateTimeOffset createdAt)
    {
        try
        {
            var sensor = await ResolveSensorAsync(sensorId).ConfigureAwait(false);

            if (sensor is null)
            {
                _logger.LogWarning("Cannot broadcast alert: SensorSnapshot not found for {SensorId}", sensorId);
                return;
            }

            var notification = new AlertCreatedNotification(
                alertId,
                sensorId,
                sensor.Label,
                sensor.PlotId,
                sensor.PlotName,
                sensor.PropertyName,
                alertType,
                severity,
                message,
                value,
                threshold,
                Metadata: null,
                createdAt);

            await _hubContext.Clients
                .Group($"plot:{sensor.PlotId}")
                .AlertCreated(notification)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Alert {AlertId} broadcast to plot {PlotId} (Type: {AlertType}, Severity: {Severity})",
                alertId,
                sensor.PlotId,
                alertType,
                severity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast alert created for {AlertId}", alertId);
        }
    }

    public async Task NotifyAlertAcknowledgedAsync(
        Guid alertId,
        Guid sensorId,
        Guid acknowledgedBy,
        DateTimeOffset acknowledgedAt)
    {
        try
        {
            var sensor = await ResolveSensorAsync(sensorId).ConfigureAwait(false);

            if (sensor is null)
            {
                _logger.LogWarning("Cannot broadcast alert acknowledgment: SensorSnapshot not found for {SensorId}", sensorId);
                return;
            }

            var notification = new AlertAcknowledgedNotification(
                alertId,
                sensorId,
                sensor.Label ?? $"Sensor {sensorId}",
                sensor.PlotId,
                sensor.PlotName,
                sensor.PropertyName,
                acknowledgedBy,
                acknowledgedAt);

            await _hubContext.Clients
                .Group($"plot:{sensor.PlotId}")
                .AlertAcknowledged(notification)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Alert {AlertId} acknowledged broadcast to plot {PlotId}",
                alertId,
                sensor.PlotId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast alert acknowledged for {AlertId}", alertId);
        }
    }

    public async Task NotifyAlertResolvedAsync(
        Guid alertId,
        Guid sensorId,
        Guid resolvedBy,
        string? resolutionNotes,
        DateTimeOffset resolvedAt)
    {
        try
        {
            var sensor = await ResolveSensorAsync(sensorId).ConfigureAwait(false);

            if (sensor is null)
            {
                _logger.LogWarning("Cannot broadcast alert resolution: SensorSnapshot not found for {SensorId}", sensorId);
                return;
            }

            var notification = new AlertResolvedNotification(
                alertId,
                sensorId,
                sensor.Label ?? $"Sensor {sensorId}",
                sensor.PlotId,
                sensor.PlotName,
                sensor.PropertyName,
                resolvedBy,
                resolutionNotes,
                resolvedAt);

            await _hubContext.Clients
                .Group($"plot:{sensor.PlotId}")
                .AlertResolved(notification)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Alert {AlertId} resolved broadcast to plot {PlotId}",
                alertId,
                sensor.PlotId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast alert resolved for {AlertId}", alertId);
        }
    }

    /// <summary>
    /// Resolves sensor snapshot from cache or database.
    /// Pattern: Same caching strategy as SensorHubNotifier.
    /// </summary>
    private async Task<SensorSnapshot?> ResolveSensorAsync(Guid sensorId)
    {
        var cacheKey = $"sensor:snapshot:{sensorId}";

        return await _cache.GetOrSetAsync<SensorSnapshot?>(
            cacheKey,
            async (_, ct) =>
            {
                var snapshot = await _snapshotStore.GetByIdAsync(sensorId, ct).ConfigureAwait(false);
                return snapshot;
            },
            new FusionCacheEntryOptions { Duration = PlotIdCacheDuration }).ConfigureAwait(false);
    }
}
