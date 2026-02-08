namespace TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

/// <summary>
/// Validator for GetPlotStatusQuery.
/// </summary>
public sealed class GetPlotStatusQueryValidator : AbstractValidator<GetPlotStatusQuery>
{
    public GetPlotStatusQueryValidator()
    {
        RuleFor(x => x.PlotId)
            .NotEmpty()
                .WithMessage("PlotId is required.")
                .WithErrorCode($"{nameof(GetPlotStatusQuery.PlotId)}.Required");
    }
}
