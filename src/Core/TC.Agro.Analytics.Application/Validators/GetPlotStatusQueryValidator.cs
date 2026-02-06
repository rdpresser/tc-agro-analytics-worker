using FluentValidation;
using TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

namespace TC.Agro.Analytics.Application.Validators;

/// <summary>
/// Validator for GetPlotStatusQuery
/// </summary>
public class GetPlotStatusQueryValidator : AbstractValidator<GetPlotStatusQuery>
{
    public GetPlotStatusQueryValidator()
    {
        RuleFor(x => x.PlotId)
            .NotEmpty()
            .WithMessage("PlotId is required");
    }
}
