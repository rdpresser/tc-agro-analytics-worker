namespace TC.Agro.Analytics.Application.MessageBrokerHandlers;

/// <summary>
/// Handler for processing sensor data ingestion events.
/// Orchestrates validation and delegates business logic to the command handler.
/// </summary>
public sealed class SensorIngestedHandler : IWolverineHandler
{
    private readonly ProcessSensorAlertsCommandHandler _commandHandler;
    private readonly IValidator<ProcessSensorAlertsCommand> _validator;
    private readonly ILogger<SensorIngestedHandler> _logger;

    public SensorIngestedHandler(
        ProcessSensorAlertsCommandHandler commandHandler,
        IValidator<ProcessSensorAlertsCommand> validator,
        ILogger<SensorIngestedHandler> logger)
    {
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(SensorIngestedIntegrationEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation(
            "🎯 Processing SensorIngestedIntegrationEvent for Sensor {SensorId}, Plot {PlotId}",
            message.SensorId,
            message.PlotId);

        var command = new ProcessSensorAlertsCommand(
            SensorId: message.SensorId,
            PlotId: message.PlotId,
            Time: message.Time,
            Temperature: message.Temperature,
            Humidity: message.Humidity,
            SoilMoisture: message.SoilMoisture,
            Rainfall: message.Rainfall,
            BatteryLevel: message.BatteryLevel);

        var validationResult = await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "⚠️ Invalid event for Sensor {SensorId}: {Errors}. Skipping processing (idempotent).",
                message.SensorId,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return;
        }

        await _commandHandler.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
    }
}
