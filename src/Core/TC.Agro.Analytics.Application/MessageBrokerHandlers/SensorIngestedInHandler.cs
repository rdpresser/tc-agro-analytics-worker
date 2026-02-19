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
        private readonly AlertThresholds _alertThresholds;

        public SensorIngestedHandler(
            ILogger<SensorIngestedHandler> logger,
            IOptions<AlertThresholdsOptions> alertThresholdsOptions,
            IAlertAggregateRepository alertRepository,
            IUnitOfWork unitOfWork)
        {
            _alertThresholds = new AlertThresholds(
                maxTemperature: alertThresholdsOptions.Value.MaxTemperature,
                minSoilMoisture: alertThresholdsOptions.Value.MinSoilMoisture,
                minBatteryLevel: alertThresholdsOptions.Value.MinBatteryLevel);
            _alertRepository = alertRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(EventContext<SensorIngestedIntegrationEvent> @event, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var alertsResult = AlertAggregate.CreateFromSensorData(
                sensorId: @event.EventData.SensorId,
                plotId: @event.EventData.PlotId,
                temperature: @event.EventData.Temperature,
                soilMoisture: @event.EventData.SoilMoisture,
                batteryLevel: @event.EventData.BatteryLevel,
                humidity: @event.EventData.Humidity,
                rainfall: @event.EventData.Rainfall,
                thresholds: _alertThresholds);

            _alertRepository.AddRange(alertsResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
