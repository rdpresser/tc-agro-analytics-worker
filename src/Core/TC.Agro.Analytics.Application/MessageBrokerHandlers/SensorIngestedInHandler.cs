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
        private readonly IAlertHubNotifier _alertHubNotifier;

        public SensorIngestedHandler(
            ILogger<SensorIngestedHandler> logger,
            IOptions<AlertThresholdOptions> alertThresholdsOptions,
            IAlertAggregateRepository alertRepository,
            IUnitOfWork unitOfWork,
            IAlertHubNotifier alertHubNotifier)
        {
            _alertThreshold = alertThresholdsOptions.Value;
            _alertRepository = alertRepository;
            _unitOfWork = unitOfWork;
            _alertHubNotifier = alertHubNotifier;
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

            if (!alertsResult.Value.Any())
                return;

            _alertRepository.AddRange(alertsResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            foreach (var alert in alertsResult.Value)
            {
                await _alertHubNotifier.NotifyAlertCreatedAsync(
                    alertId: alert.Id,
                    sensorId: alert.SensorId,
                    alertType: alert.Type.ToString(),
                    severity: alert.Severity.ToString(),
                    message: alert.Message,
                    value: alert.Value,
                    threshold: alert.Threshold,
                    createdAt: alert.CreatedAt
                ).ConfigureAwait(false);
            }
        }
    }
}
