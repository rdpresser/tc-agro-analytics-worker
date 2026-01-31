using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TC.Agro.Analytics.Application.Configuration;
using TC.Agro.Analytics.Domain.Abstractions.Ports;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Domain.ValueObjects;
using TC.Agro.Contracts.Events;
using TC.Agro.Contracts.Events.Analytics;
using TC.Agro.SharedKernel.Domain.Events;
using TC.Agro.SharedKernel.Infrastructure.Messaging;
using Wolverine;
using Wolverine.Marten;

namespace TC.Agro.Analytics.Application.MessageBrokerHandlers
{
    public class SensorIngestedHandler : IWolverineHandler
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly ILogger<SensorIngestedHandler> _logger;
        private readonly IMartenOutbox _outbox;
        private readonly AlertThresholds _alertThresholds;

        public SensorIngestedHandler(
            ISensorReadingRepository sensorReadingRepository,
            IMartenOutbox outbox,
            ILogger<SensorIngestedHandler> logger,
            IOptions<AlertThresholdsOptions> alertThresholdsOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sensorReadingRepository = sensorReadingRepository ?? throw new ArgumentNullException(nameof(sensorReadingRepository));
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));

            // Map configuration options to domain value object
            var options = alertThresholdsOptions?.Value ?? throw new ArgumentNullException(nameof(alertThresholdsOptions));
            _alertThresholds = new AlertThresholds(
                maxTemperature: options.MaxTemperature,
                minSoilMoisture: options.MinSoilMoisture,
                minBatteryLevel: options.MinBatteryLevel);
        }
        public async Task HandleAsync(EventContext<SensorIngestedIntegrationEvent> @event, CancellationToken cancellationToken = default)
        {
            try
            {
                //1. Check for existing aggregate to avoid duplicates
                var existingAggregate = await _sensorReadingRepository.GetByIdAsync(@event.EventData.AggregateId, cancellationToken);
                
                if (existingAggregate != null)
                {
                    _logger.LogWarning("Duplicate event detected: {MessageId}", @event.MessageId);
                    return; // Idempotent
                }

                //1. Map event to domain aggregate
                var aggregate = MapEventToAggregate(@event);

                //2. Evaluate alerts using configured thresholds (Domain logic - DDD)
                aggregate.EvaluateAlerts(_alertThresholds);

                //3. Persist domain aggregate with uncommitted events
                await _sensorReadingRepository.SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);

                //4. Publish integration events (map domain events -> integration events)
                await PublishIntegrationEventsAsync(aggregate).ConfigureAwait(false);

                //5. Commit transaction with outbox pattern
                await _sensorReadingRepository.CommitAsync(aggregate, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Sensor reading processed successfully for Sensor {SensorId}, Plot {PlotId}", 
                    aggregate.SensorId, 
                    aggregate.PlotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing SensorIngestedIntegrationEvent for Sensor {SensorId}, Plot {PlotId}", 
                    @event.EventData.SensorId,
                    @event.EventData.PlotId);

                throw new InvalidOperationException(
                    $"Failed to process sensor reading for Sensor {@event.EventData.SensorId}, Plot {@event.EventData.PlotId}", 
                    ex);
            }
        }

        private static SensorReadingAggregate MapEventToAggregate(EventContext<SensorIngestedIntegrationEvent> @event)
        {
            var result = SensorReadingAggregate.Create(
                sensorId: @event.EventData.SensorId,
                plotId: @event.EventData.PlotId,
                time: @event.EventData.Time,
                temperature: @event.EventData.Temperature,
                humidity: @event.EventData.Humidity,
                soilMoisture: @event.EventData.SoilMoisture,
                rainfall: @event.EventData.Rainfall,
                batteryLevel: @event.EventData.BatteryLevel
            );

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException($"Failed to create SensorReadingAggregate: {string.Join(", ", result.ValidationErrors.Select(e => e.ErrorMessage))}");
            }

            return result.Value;
        }

        protected async Task PublishIntegrationEventsAsync(SensorReadingAggregate aggregate)
        {
            // Map domain events to integration events and publish
            var mappings = new Dictionary<Type, Func<BaseDomainEvent, BaseIntegrationEvent>>
            {
                { typeof(SensorReadingAggregate.HighTemperatureDetectedDomainEvent), 
                  e => MapToHighTemperatureIntegrationEvent((SensorReadingAggregate.HighTemperatureDetectedDomainEvent)e) },
                { typeof(SensorReadingAggregate.LowSoilMoistureDetectedDomainEvent), 
                  e => MapToLowSoilMoistureIntegrationEvent((SensorReadingAggregate.LowSoilMoistureDetectedDomainEvent)e) },
                { typeof(SensorReadingAggregate.BatteryLowWarningDomainEvent), 
                  e => MapToBatteryLowWarningIntegrationEvent((SensorReadingAggregate.BatteryLowWarningDomainEvent)e) }
            };

            var integrationEvents = aggregate.UncommittedEvents
                .Where(e => e is SensorReadingAggregate.HighTemperatureDetectedDomainEvent 
                         or SensorReadingAggregate.LowSoilMoistureDetectedDomainEvent 
                         or SensorReadingAggregate.BatteryLowWarningDomainEvent)
                .Select(domainEvent =>
                {
                    var eventType = domainEvent.GetType();
                    return mappings.TryGetValue(eventType, out var mapper) ? mapper(domainEvent) : null;
                })
                .Where(evt => evt != null);

            foreach (var evt in integrationEvents)
            {
                await _outbox.PublishAsync(evt!).ConfigureAwait(false);

                // Log alert publication
                _logger.LogWarning(
                    "Alert published: {EventType} for Sensor {SensorId}",
                    evt!.GetType().Name,
                    GetSensorIdFromEvent(evt));
            }
        }

        private static string GetSensorIdFromEvent(BaseIntegrationEvent evt)
        {
            return evt switch
            {
                HighTemperatureDetectedIntegrationEvent e => e.SensorId,
                LowSoilMoistureDetectedIntegrationEvent e => e.SensorId,
                BatteryLowWarningIntegrationEvent e => e.SensorId,
                _ => "Unknown"
            };
        }

        private static HighTemperatureDetectedIntegrationEvent MapToHighTemperatureIntegrationEvent(
            SensorReadingAggregate.HighTemperatureDetectedDomainEvent domainEvent)
            => new(
                EventId: Guid.NewGuid(),
                AggregateId: domainEvent.AggregateId,
                OccurredOn: domainEvent.OccurredOn,
                EventName: nameof(HighTemperatureDetectedIntegrationEvent),
                RelatedIds: new Dictionary<string, Guid>
                {
                    { "SensorReadingId", domainEvent.AggregateId },
                    { "PlotId", domainEvent.PlotId }
                },
                SensorId: domainEvent.SensorId,
                PlotId: domainEvent.PlotId,
                Time: domainEvent.Time,
                Temperature: domainEvent.Temperature,
                Humidity: domainEvent.Humidity,
                SoilMoisture: domainEvent.SoilMoisture,
                Rainfall: domainEvent.Rainfall,
                BatteryLevel: domainEvent.BatteryLevel
            );

        private static LowSoilMoistureDetectedIntegrationEvent MapToLowSoilMoistureIntegrationEvent(
            SensorReadingAggregate.LowSoilMoistureDetectedDomainEvent domainEvent)
            => new(
                EventId: Guid.NewGuid(),
                AggregateId: domainEvent.AggregateId,
                OccurredOn: domainEvent.OccurredOn,
                EventName: nameof(LowSoilMoistureDetectedIntegrationEvent),
                RelatedIds: new Dictionary<string, Guid>
                {
                    { "SensorReadingId", domainEvent.AggregateId },
                    { "PlotId", domainEvent.PlotId }
                },
                SensorId: domainEvent.SensorId,
                PlotId: domainEvent.PlotId,
                Time: domainEvent.Time,
                Temperature: domainEvent.Temperature,
                Humidity: domainEvent.Humidity,
                SoilMoisture: domainEvent.SoilMoisture,
                Rainfall: domainEvent.Rainfall,
                BatteryLevel: domainEvent.BatteryLevel
            );

        private static BatteryLowWarningIntegrationEvent MapToBatteryLowWarningIntegrationEvent(
            SensorReadingAggregate.BatteryLowWarningDomainEvent domainEvent)
            => new(
                EventId: Guid.NewGuid(),
                AggregateId: domainEvent.AggregateId,
                OccurredOn: domainEvent.OccurredOn,
                EventName: nameof(BatteryLowWarningIntegrationEvent),
                RelatedIds: new Dictionary<string, Guid>
                {
                    { "SensorReadingId", domainEvent.AggregateId },
                    { "PlotId", domainEvent.PlotId }
                },
                SensorId: domainEvent.SensorId,
                PlotId: domainEvent.PlotId,
                BatteryLevel: domainEvent.BatteryLevel,
                Threshold: domainEvent.Threshold
            );
    }
}
