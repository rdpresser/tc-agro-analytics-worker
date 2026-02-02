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
using Wolverine;

namespace TC.Agro.Analytics.Application.MessageBrokerHandlers
{
    public class SensorIngestedHandler
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly ILogger<SensorIngestedHandler> _logger;
        private readonly AlertThresholds _alertThresholds;
        private readonly IMessageBus _messageBus; // Wolverine Message Bus para publicar eventos

        public SensorIngestedHandler(
            ISensorReadingRepository sensorReadingRepository,
            ILogger<SensorIngestedHandler> logger,
            IOptions<AlertThresholdsOptions> alertThresholdsOptions,
            IMessageBus messageBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sensorReadingRepository = sensorReadingRepository ?? throw new ArgumentNullException(nameof(sensorReadingRepository));
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

            // Map configuration options to domain value object
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

                //1. Check for existing aggregate to avoid duplicates
                var existingAggregate = await _sensorReadingRepository.GetByIdAsync(message.AggregateId, cancellationToken);

                if (existingAggregate != null)
                {
                    _logger.LogWarning("Duplicate event detected: {AggregateId}", message.AggregateId);
                    return; // Idempotent
                }

                //2. Map event to domain aggregate
                var aggregate = MapEventToAggregate(message);

                //3. Evaluate alerts using configured thresholds (Domain logic - DDD)
                aggregate.EvaluateAlerts(_alertThresholds);

                //4. Persist domain aggregate with uncommitted events
                await _sensorReadingRepository.SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);

                //5. PUBLICAR Domain Events para Wolverine ANTES do commit (Transactional Outbox)
                await PublishDomainEventsAsync(aggregate);

                //6. Commit transaction with outbox pattern
                await _sensorReadingRepository.CommitAsync(aggregate, cancellationToken).ConfigureAwait(false);

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
            var result = SensorReadingAggregate.Create(
                sensorId: message.SensorId,
                plotId: message.PlotId,
                time: message.Time,
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
        /// Publica Domain Events para Wolverine via Transactional Outbox
        /// </summary>
        private async Task PublishDomainEventsAsync(SensorReadingAggregate aggregate)
        {
            var domainEvents = aggregate.UncommittedEvents?.ToList();
            if (domainEvents == null || !domainEvents.Any())
                return;

            foreach (var domainEvent in domainEvents)
            {
                _logger.LogInformation("📤 Publishing domain event: {EventType}", domainEvent.GetType().Name);

                // Wolverine Transactional Outbox - eventos serão publicados após commit
                await _messageBus.PublishAsync(domainEvent);
            }

            _logger.LogInformation("✅ Published {Count} domain event(s) to Wolverine", domainEvents.Count);
        }
    }
}
