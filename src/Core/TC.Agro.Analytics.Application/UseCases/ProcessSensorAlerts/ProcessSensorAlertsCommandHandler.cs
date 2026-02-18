namespace TC.Agro.Analytics.Application.UseCases.ProcessSensorAlerts;

/// <summary>
/// Handler for processing sensor alerts based on ingested sensor data.
/// Encapsulates the business logic for alert creation and persistence.
/// Invalidates cache when new alerts are created.
/// </summary>
public sealed class ProcessSensorAlertsCommandHandler
{
    private readonly IAlertAggregateRepository _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProcessSensorAlertsCommandHandler> _logger;
    private readonly AlertThresholds _alertThresholds;

    public ProcessSensorAlertsCommandHandler(
        IAlertAggregateRepository alertRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ProcessSensorAlertsCommandHandler> logger,
        IOptions<AlertThresholdsOptions> alertThresholdsOptions)
    {
        _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(alertThresholdsOptions);

        _alertThresholds = new AlertThresholds(
            maxTemperature: alertThresholdsOptions.Value.MaxTemperature,
            minSoilMoisture: alertThresholdsOptions.Value.MinSoilMoisture,
            minBatteryLevel: alertThresholdsOptions.Value.MinBatteryLevel);
    }

    public async Task ExecuteAsync(
        ProcessSensorAlertsCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var alertsResult = AlertAggregate.CreateFromSensorData(
            sensorId: command.SensorId,
            plotId: command.PlotId,
            temperature: command.Temperature,
            soilMoisture: command.SoilMoisture,
            batteryLevel: command.BatteryLevel,
            humidity: command.Humidity,
            rainfall: command.Rainfall,
            thresholds: _alertThresholds);

        if (!alertsResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to create alerts for Sensor {SensorId}: {Errors}",
                command.SensorId,
                string.Join(", ", alertsResult.ValidationErrors.Select(e => e.ErrorMessage)));
            return;
        }

        if (alertsResult.Value.Count == 0)
        {
            _logger.LogDebug("No alerts triggered for Sensor {SensorId}", command.SensorId);
            return;
        }

        try
        {
            await PersistAlertsAsync(alertsResult.Value, command.SensorId, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "✅ Successfully processed {Count} alerts for Sensor {SensorId}",
                alertsResult.Value.Count,
                command.SensorId);
        }
#pragma warning disable S2139 // Exception is logged with context before rethrowing for Wolverine retry mechanism
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Failed to persist alerts for Sensor {SensorId}. This message will be retried.",
                command.SensorId);
            throw;
        }
#pragma warning restore S2139
    }

    private async Task PersistAlertsAsync(
        IReadOnlyList<AlertAggregate> alerts,
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        foreach (var alert in alerts)
        {
            _alertRepository.Add(alert);

            _logger.LogInformation(
                "📝 Created {AlertType} alert (Severity: {Severity}) for Sensor {SensorId}",
                alert.Type.Value,
                alert.Severity.Value,
                sensorId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Invalidate cache after creating new alerts
        if (alerts.Count > 0)
        {
            await _cacheService.RemoveByTagAsync("GetPendingAlertsQuery", cancellationToken).ConfigureAwait(false);

            // Invalidate plot-specific caches
            foreach (var plotId in alerts.Select(a => a.PlotId).Distinct())
            {
                await _cacheService.RemoveByTagAsync($"plot-{plotId}", cancellationToken).ConfigureAwait(false);
            }

            _logger.LogInformation("Cache invalidated for {Count} new alerts", alerts.Count);
        }
    }
}
