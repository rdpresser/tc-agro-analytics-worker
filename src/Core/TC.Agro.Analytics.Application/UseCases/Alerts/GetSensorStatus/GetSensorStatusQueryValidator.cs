namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetSensorStatus;

/// <summary>
/// Validator for GetSensorStatusQuery.
/// </summary>
public sealed class GetSensorStatusQueryValidator : AbstractValidator<GetSensorStatusQuery>
{
    public GetSensorStatusQueryValidator()
    {
        RuleFor(x => x.SensorId)
            .NotEmpty()
                .WithMessage("SensorId is required.")
                .WithErrorCode($"{nameof(GetSensorStatusQuery.SensorId)}.Required");
    }
}
