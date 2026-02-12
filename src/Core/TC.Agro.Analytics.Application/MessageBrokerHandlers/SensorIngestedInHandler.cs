using Wolverine;

namespace TC.Agro.Analytics.Application.MessageBrokerHandlers;

/// <summary>
/// Handler for processing sensor data ingestion events.
/// 
/// ARCHITECTURAL DECISION - Bounded Context Ownership (per Domain Map):
/// - Sensor.Ingest.Api OWNS SensorReading (TimescaleDB source of truth)
/// - Analytics.Worker OWNS Alert (rule evaluation and alert lifecycle)
/// 
/// This handler does NOT persist SensorReading (already persisted by Ingest.Api).
/// It only evaluates rules and creates AlertAggregates when thresholds are violated.
/// 
/// Pattern: Similar to Farm Service OwnerSnapshotHandler
/// - Consumes integration event (SensorIngestedIntegrationEvent)
/// - Evaluates business rules (global AlertThresholds from config)
/// - Creates AlertAggregate (with Pending status)
/// - Persists ONLY Alert (not SensorReading - ownership violation!)
/// - Uses IUnitOfWork (transactional consistency)
/// 
/// Following Farm Service pattern: implements IWolverineHandler.
/// </summary>
public class SensorIngestedHandler : IWolverineHandler
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

            // 1. Validate event data
            var validationResult = ValidateEvent(message);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning(
                    "⚠️ Invalid event data for Sensor {SensorId}, Plot {PlotId}: {Errors}. Skipping processing (idempotent).",
                    message.SensorId,
                    message.PlotId,
                    string.Join(", ", validationResult.ValidationErrors.Select(e => e.ErrorMessage)));
                return;
            }

            // 2. Evaluate alert rules (stateless - using event data, NOT persisting SensorReading!)
            var alertsToCreate = EvaluateAlertRules(message, _alertThresholds);

            if (!alertsToCreate.Any())
            {
                _logger.LogDebug("No alerts triggered for Sensor {SensorId}", message.SensorId);
                return;
            }

            // 3. Create AlertAggregates
            foreach (var alertData in alertsToCreate)
            {
                var alertResult = AlertAggregate.Create(
                    sensorId: message.SensorId,
                    plotId: message.PlotId,
                    type: alertData.Type,
                    severity: alertData.Severity,
                    message: alertData.Message,
                    value: alertData.Value,
                    threshold: alertData.Threshold,
                    metadata: alertData.Metadata);

                if (!alertResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to create alert for Sensor {SensorId}: {Errors}",
                        message.SensorId,
                        string.Join(", ", alertResult.ValidationErrors.Select(e => e.ErrorMessage)));
                    continue;
                }

                _alertRepository.Add(alertResult.Value);

                _logger.LogInformation(
                    "📝 Created {AlertType} alert (Severity: {Severity}) for Sensor {SensorId}",
                    alertData.Type.Value,
                    alertData.Severity.Value,
                    message.SensorId);
            }

            // 4. Commit (ONLY alerts, NO sensor_readings - per Domain Map ownership!)
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "✅ Processed {Count} alerts for Sensor {SensorId}",
                alertsToCreate.Count,
                message.SensorId);
        }

            /// <summary>
            /// Validates event data.
            /// Returns Result pattern - NO exceptions thrown for validation errors.
            /// </summary>
            private static Result ValidateEvent(SensorIngestedIntegrationEvent evt)
            {
                var errors = new List<ValidationError>();

                if (string.IsNullOrWhiteSpace(evt.SensorId))
                    errors.Add(new ValidationError("SensorId.Required", "SensorId is required"));

                if (evt.PlotId == Guid.Empty)
                    errors.Add(new ValidationError("PlotId.Required", "PlotId is required"));

                if (evt.Time == default)
                    errors.Add(new ValidationError("Time.Required", "Time is required"));

                if (!evt.Temperature.HasValue && !evt.Humidity.HasValue &&
                    !evt.SoilMoisture.HasValue && !evt.Rainfall.HasValue)
                    errors.Add(new ValidationError("Metrics.Required", "At least one metric is required"));

                return errors.Any()
                    ? Result.Invalid(errors.ToArray())
                    : Result.Success();
            }

            /// <summary>
            /// Evaluates alert rules against event data (stateless).
            /// Returns list of alerts to create (NOT persisted yet).
            /// Following global thresholds pattern (YAGNI - no per-plot rules table for MVP).
            /// </summary>
            private static IReadOnlyList<AlertData> EvaluateAlertRules(
                SensorIngestedIntegrationEvent evt,
                AlertThresholds thresholds)
            {
                var alerts = new List<AlertData>();

                // Rule 1: High Temperature
                if (evt.Temperature.HasValue && evt.Temperature.Value > thresholds.MaxTemperature)
                {
                    var severity = CalculateTemperatureSeverity(evt.Temperature.Value, thresholds.MaxTemperature);
                    alerts.Add(new AlertData(
                        Type: AlertType.HighTemperature,
                        Severity: severity,
                        Message: $"High temperature detected: {evt.Temperature:F1}°C",
                        Value: evt.Temperature.Value,
                        Threshold: thresholds.MaxTemperature,
                        Metadata: System.Text.Json.JsonSerializer.Serialize(new
                        {
                            evt.Humidity,
                            evt.SoilMoisture,
                            evt.Rainfall,
                            evt.BatteryLevel
                        })));
                }

                // Rule 2: Low Soil Moisture
                if (evt.SoilMoisture.HasValue && evt.SoilMoisture.Value < thresholds.MinSoilMoisture)
                {
                    var severity = CalculateSoilMoistureSeverity(evt.SoilMoisture.Value, thresholds.MinSoilMoisture);
                    alerts.Add(new AlertData(
                        Type: AlertType.LowSoilMoisture,
                        Severity: severity,
                        Message: $"Low soil moisture detected: {evt.SoilMoisture:F1}% - Irrigation may be needed",
                        Value: evt.SoilMoisture.Value,
                        Threshold: thresholds.MinSoilMoisture,
                        Metadata: System.Text.Json.JsonSerializer.Serialize(new
                        {
                            evt.Temperature,
                            evt.Humidity,
                            evt.Rainfall,
                            evt.BatteryLevel
                        })));
                }

                // Rule 3: Low Battery
                if (evt.BatteryLevel.HasValue && evt.BatteryLevel.Value < thresholds.MinBatteryLevel)
                {
                    var severity = CalculateBatterySeverity(evt.BatteryLevel.Value);
                    alerts.Add(new AlertData(
                        Type: AlertType.LowBattery,
                        Severity: severity,
                        Message: $"Low battery warning: {evt.BatteryLevel:F1}% - Sensor maintenance required",
                        Value: evt.BatteryLevel.Value,
                        Threshold: thresholds.MinBatteryLevel,
                        Metadata: System.Text.Json.JsonSerializer.Serialize(new
                        {
                            Threshold = thresholds.MinBatteryLevel
                        })));
                }

                return alerts;
            }

            private static AlertSeverity CalculateTemperatureSeverity(double temperature, double threshold)
            {
                var excess = temperature - threshold;
                return excess switch
                {
                    >= 15 => AlertSeverity.Critical,
                    >= 10 => AlertSeverity.High,
                    >= 5 => AlertSeverity.Medium,
                    _ => AlertSeverity.Low
                };
            }

            private static AlertSeverity CalculateSoilMoistureSeverity(double soilMoisture, double threshold)
            {
                var deficit = threshold - soilMoisture;
                return deficit switch
                {
                    >= 30 => AlertSeverity.Critical,
                    >= 20 => AlertSeverity.High,
                    >= 10 => AlertSeverity.Medium,
                    _ => AlertSeverity.Low
                };
            }

            private static AlertSeverity CalculateBatterySeverity(double batteryLevel)
            {
                return batteryLevel switch
                {
                    < 10 => AlertSeverity.Critical,
                    < 20 => AlertSeverity.High,
                    < 30 => AlertSeverity.Medium,
                    _ => AlertSeverity.Low
                };
            }

                private sealed record AlertData(
                    AlertType Type,
                    AlertSeverity Severity,
                    string Message,
                    double Value,
                    double Threshold,
                    string? Metadata);
            }
