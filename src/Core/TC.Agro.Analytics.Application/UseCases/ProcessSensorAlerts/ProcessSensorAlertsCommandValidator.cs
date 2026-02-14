namespace TC.Agro.Analytics.Application.UseCases.ProcessSensorAlerts;

/// <summary>
/// Validator for ProcessSensorAlertsCommand using FluentValidation.
/// </summary>
public sealed class ProcessSensorAlertsCommandValidator : AbstractValidator<ProcessSensorAlertsCommand>
{
    public ProcessSensorAlertsCommandValidator()
    {
        RuleFor(x => x.SensorId)
            .NotEmpty()
            .WithMessage("SensorId is required.");

        RuleFor(x => x.PlotId)
            .NotEmpty()
            .WithMessage("PlotId is required.");

        RuleFor(x => x.Time)
            .NotEmpty()
            .WithMessage("Time is required.");

        RuleFor(x => x)
            .Must(HasAtLeastOneMetric)
            .WithMessage("At least one metric (Temperature, Humidity, SoilMoisture, or Rainfall) must be provided.");
    }

    private static bool HasAtLeastOneMetric(ProcessSensorAlertsCommand command) =>
        command.Temperature.HasValue ||
        command.Humidity.HasValue ||
        command.SoilMoisture.HasValue ||
        command.Rainfall.HasValue;
}
