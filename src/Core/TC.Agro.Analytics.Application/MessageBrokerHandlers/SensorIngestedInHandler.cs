using TC.Agro.Analytics.Application.Abstractions.Options.AlertThreshold;

namespace TC.Agro.Analytics.Application.MessageBrokerHandlers
{
    /// <summary>
    /// Handler for processing sensor data ingestion events.
    /// Orchestrates validation and delegates business logic to the command handler.
    /// </summary>
    public sealed class SensorIngestedHandler : IWolverineHandler
    {
        private readonly IAlertAggregateRepository _alertRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AlertThresholdOptions _alertThreshold;

        public SensorIngestedHandler(
            ILogger<SensorIngestedHandler> logger,
            IOptions<AlertThresholdOptions> alertThresholdsOptions,
            IAlertAggregateRepository alertRepository,
            IUnitOfWork unitOfWork)
        {
            _alertThreshold = alertThresholdsOptions.Value;
            _alertRepository = alertRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(EventContext<SensorIngestedIntegrationEvent> @event, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var alertsResult = AlertAggregate.CreateFromSensorData(
                sensorId: @event.EventData.SensorId,
                temperature: @event.EventData.Temperature,
                soilMoisture: @event.EventData.SoilMoisture,
                batteryLevel: @event.EventData.BatteryLevel,
                humidity: @event.EventData.Humidity,
                rainfall: @event.EventData.Rainfall,
                maxTemperature: _alertThreshold.MaxTemperature,
                minSoilMoisture: _alertThreshold.MinSoilMoisture,
                minBatteryLevel: _alertThreshold.MinBatteryLevel);

            _alertRepository.AddRange(alertsResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
