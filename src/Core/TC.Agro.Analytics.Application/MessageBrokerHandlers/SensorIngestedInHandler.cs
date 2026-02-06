using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TC.Agro.Analytics.Application.Configuration;
using TC.Agro.Analytics.Domain.Abstractions.Ports;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Domain.ValueObjects;
using TC.Agro.Contracts.Events.Analytics;
using TC.Agro.SharedKernel.Application.Ports;

namespace TC.Agro.Analytics.Application.MessageBrokerHandlers
{
    /// <summary>
    /// Handler for processing sensor data ingestion events.
    /// Uses EF Core + Wolverine Transactional Outbox (same pattern as Identity-Service).
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

                // 1. Check for existing aggregate (idempotency)
                var existingAggregate = await _repository.GetByIdAsync(message.AggregateId, cancellationToken);
                if (existingAggregate != null)
                {
                    _logger.LogWarning("Duplicate event detected: {AggregateId}", message.AggregateId);
                    return;
                }

                // 2. Map event to domain aggregate
                var aggregate = MapEventToAggregate(message);

                // 3. Evaluate alerts (domain logic)
                aggregate.EvaluateAlerts(_alertThresholds);

                // 4. Persist aggregate (marks as Added in EF Core DbContext)
                _repository.Add(aggregate);

                // 5. Publish domain events to Wolverine Outbox
                await PublishDomainEventsAsync(aggregate, cancellationToken);

                // 6. Commit (single transaction: EF Core SaveChanges + Wolverine Outbox Flush)
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

        private static SensorReadingAggregate MapEventToAggregate(SensorIngestedIntegrationEvent message)
        {
            // Ensure DateTime is UTC (PostgreSQL timestamptz requires UTC)
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
                throw new InvalidOperationException($"Failed to create SensorReadingAggregate: {string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage))}");
            }

            return result.Value;
        }

        /// <summary>
        /// Enqueue domain events to Wolverine Transactional Outbox.
        /// Events will be published AFTER EF Core commits successfully.
        /// </summary>
        private async Task PublishDomainEventsAsync(
            SensorReadingAggregate aggregate, 
            CancellationToken cancellationToken)
        {
            var domainEvents = aggregate.UncommittedEvents?.ToList();
            if (domainEvents == null || !domainEvents.Any())
                return;

            foreach (var domainEvent in domainEvents)
            {
                _logger.LogInformation(
                    "📤 Enqueuing domain event: {EventType}", 
                    domainEvent.GetType().Name);

                // Wolverine Transactional Outbox (EF Core integration)
                await _outbox.EnqueueAsync(domainEvent, cancellationToken);
            }

            _logger.LogInformation(
                "✅ Enqueued {Count} domain event(s) to Wolverine Outbox", 
                domainEvents.Count);
        }
    }
}
