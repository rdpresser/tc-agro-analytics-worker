namespace TC.Agro.Analytics.Application.MessageBrokerHandlers;

/// <summary>
/// Handler for processing sensor data ingestion events.
/// Pattern: Similar to CreatePlotCommandHandler and CreateUserCommandHandler
/// - Maps event to aggregate
/// - Validates business rules (using injected global AlertThresholds)
/// - Persists aggregate and related entities (alerts) in a single transaction
/// - Does NOT publish domain events to external systems
/// </summary>
public class SensorIngestedHandler
    {
        private readonly ISensorReadingRepository _repository;
        private readonly ITransactionalOutbox _outbox;
        private readonly ILogger<SensorIngestedHandler> _logger;
        private readonly AlertThresholds _alertThresholds;

        public SensorIngestedHandler(
            ISensorReadingRepository repository,
            ITransactionalOutbox outbox,
            ILogger<SensorIngestedHandler> logger,
            IOptions<AlertThresholdsOptions> alertThresholdsOptions)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var options = alertThresholdsOptions?.Value ?? throw new ArgumentNullException(nameof(alertThresholdsOptions));
            _alertThresholds = new AlertThresholds(
                maxTemperature: options.MaxTemperature,
                minSoilMoisture: options.MinSoilMoisture,
                minBatteryLevel: options.MinBatteryLevel);
        }

        public async Task Handle(SensorIngestedIntegrationEvent message, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "🎯 Processing SensorIngestedIntegrationEvent for Sensor {SensorId}, Plot {PlotId}", 
                    message.SensorId, 
                    message.PlotId);

                // 1. Map event to domain aggregate
                var aggregate = await MapAsync(message);

                // 2. Validate (idempotency check)
                var validationResult = await ValidateAsync(aggregate, cancellationToken);
                if (!validationResult)
                {
                    return; // Duplicate detected, skip processing
                }

                // 3. Execute business logic (evaluate alerts with GLOBAL thresholds from config)
                aggregate.EvaluateAlerts(_alertThresholds);

                // 4. Persist aggregate
                _repository.Add(aggregate);

                // 5. Create related entities (alerts) - following the pattern
                //    Domain: generates events
                //    Application: creates read models from those events
                await CreateRelatedEntitiesAsync(aggregate, cancellationToken);

                // 6. Commit transaction
                await _outbox.SaveChangesAsync(cancellationToken);

                // 7. Mark events as committed
                aggregate.MarkEventsAsCommitted();

                _logger.LogInformation(
                    "✅ Sensor reading processed successfully for Sensor {SensorId}, Plot {PlotId}", 
                    aggregate.SensorId, 
                    aggregate.PlotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ Error processing SensorIngestedIntegrationEvent for Sensor {SensorId}, Plot {PlotId}", 
                    message.SensorId,
                    message.PlotId);

                throw new InvalidOperationException(
                    $"Failed to process sensor reading for Sensor {message.SensorId}, Plot {message.PlotId}", 
                    ex);
            }
        }

        /// <summary>
        /// Maps integration event to domain aggregate.
        /// Similar to MapAsync in BaseCommandHandler.
        /// </summary>
        private static Task<SensorReadingAggregate> MapAsync(SensorIngestedIntegrationEvent message)
        {
            var timeUtc = message.Time.Kind == DateTimeKind.Utc 
                ? message.Time 
                : message.Time.ToUniversalTime();

            var result = SensorReadingAggregate.Create(
                sensorId: message.SensorId,
                plotId: message.PlotId,
                time: timeUtc,
                temperature: message.Temperature,
                humidity: message.Humidity,
                soilMoisture: message.SoilMoisture,
                rainfall: message.Rainfall,
                batteryLevel: message.BatteryLevel
            );

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Failed to create SensorReadingAggregate: {string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage))}");
            }

            return Task.FromResult(result.Value);
        }

        /// <summary>
        /// Validates business rules (idempotency).
        /// Similar to ValidateAsync in BaseCommandHandler.
        /// </summary>
        private async Task<bool> ValidateAsync(
            SensorReadingAggregate aggregate, 
            CancellationToken cancellationToken)
        {
            // Check for existing aggregate (idempotency)
            var existingAggregate = await _repository.GetByIdAsync(aggregate.Id, cancellationToken);
            if (existingAggregate != null)
            {
                _logger.LogWarning("Duplicate event detected: {AggregateId}", aggregate.Id);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates related entities (alerts) from domain events.
        /// Pattern: Domain logic generates events, Application layer creates read models.
        /// This is similar to how CreatePlotCommandHandler persists the aggregate,
        /// but here we also create related Alert entities in the same transaction.
        /// 
        /// Note: We don't publish these as integration events because:
        /// - Alerts are tightly coupled to sensor readings
        /// - No other services need to be notified about individual alerts
        /// - This simplifies the architecture (YAGNI principle)
        /// </summary>
        private async Task CreateRelatedEntitiesAsync(
            SensorReadingAggregate aggregate, 
            CancellationToken cancellationToken)
        {
            // Get alerts from domain events (Domain → Application boundary)
            var alerts = aggregate.GetPendingAlerts();

            if (!alerts.Any())
            {
                return;
            }

            foreach (var alert in alerts)
            {
                await _repository.AddAlertAsync(alert, cancellationToken);

                _logger.LogInformation(
                    "📝 Created {AlertType} alert (Severity: {Severity}) for Sensor {SensorId}",
                    alert.AlertType,
                    alert.Severity,
                    alert.SensorId);
            }

            _logger.LogDebug(
                "Created {Count} alerts for SensorReading {AggregateId}",
                alerts.Count,
                aggregate.Id);
        }
    }
