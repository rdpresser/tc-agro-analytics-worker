namespace TC.Agro.Analytics.Application.MessageBrokerHandlers;

/// <summary>
/// Handler for processing sensor data ingestion events.
/// </summary>
public sealed class SensorIngestedHandler : IWolverineHandler
{
    private readonly IAlertAggregateRepository _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SensorIngestedHandler> _logger;
    private readonly AlertThresholds _alertThresholds;

    public SensorIngestedHandler(
        IAlertAggregateRepository alertRepository,
        IUnitOfWork unitOfWork,
        ILogger<SensorIngestedHandler> logger,
        IOptions<AlertThresholdsOptions> alertThresholdsOptions)
    {
        _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(alertThresholdsOptions);

        _alertThresholds = new AlertThresholds(
            maxTemperature: alertThresholdsOptions.Value.MaxTemperature,
            minSoilMoisture: alertThresholdsOptions.Value.MinSoilMoisture,
            minBatteryLevel: alertThresholdsOptions.Value.MinBatteryLevel);
    }

    public async Task Handle(SensorIngestedIntegrationEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation(
            "🎯 Processing SensorIngestedIntegrationEvent for Sensor {SensorId}, Plot {PlotId}",
            message.SensorId,
            message.PlotId);

        if (!IsValidMessage(message))
        {
            return;
        }

        var alertsResult = AlertAggregate.CreateFromSensorData(
            sensorId: message.SensorId,
            plotId: message.PlotId,
            temperature: message.Temperature,
            soilMoisture: message.SoilMoisture,
            batteryLevel: message.BatteryLevel,
            humidity: message.Humidity,
            rainfall: message.Rainfall,
            thresholds: _alertThresholds);

        if (!alertsResult.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to create alerts for Sensor {SensorId}: {Errors}",
                message.SensorId,
                string.Join(", ", alertsResult.ValidationErrors.Select(e => e.ErrorMessage)));
            return;
        }

        if (alertsResult.Value.Count == 0)
        {
            _logger.LogDebug("No alerts triggered for Sensor {SensorId}", message.SensorId);
            return;
        }

        await PersistAlertsAsync(alertsResult.Value, message.SensorId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "✅ Processed {Count} alerts for Sensor {SensorId}",
            alertsResult.Value.Count,
            message.SensorId);
    }

    private bool IsValidMessage(SensorIngestedIntegrationEvent message)
    {
        if (string.IsNullOrWhiteSpace(message.SensorId))
        {
            _logger.LogWarning(
                "⚠️ Invalid event: SensorId is missing. Skipping processing (idempotent).");
            return false;
        }

        if (message.PlotId == Guid.Empty)
        {
            _logger.LogWarning(
                "⚠️ Invalid event: PlotId is empty for Sensor {SensorId}. Skipping processing (idempotent).",
                message.SensorId);
            return false;
        }

        if (message.Time == default)
        {
            _logger.LogWarning(
                "⚠️ Invalid event: Time is missing for Sensor {SensorId}. Skipping processing (idempotent).",
                message.SensorId);
            return false;
        }

        if (!message.Temperature.HasValue &&
            !message.Humidity.HasValue &&
            !message.SoilMoisture.HasValue &&
            !message.Rainfall.HasValue)
        {
            _logger.LogWarning(
                "⚠️ No sensor metrics provided for Sensor {SensorId}. Skipping processing (idempotent).",
                message.SensorId);
            return false;
        }

        return true;
    }

    private async Task PersistAlertsAsync(
        IReadOnlyList<AlertAggregate> alerts,
        string sensorId,
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
    }
}
