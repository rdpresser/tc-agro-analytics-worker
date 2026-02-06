using FluentValidation;
using TC.Agro.Analytics.Application.UseCases.GetAlertHistory;

namespace TC.Agro.Analytics.Application.Validators;

/// <summary>
/// Validator for GetAlertHistoryQuery
/// </summary>
public class GetAlertHistoryQueryValidator : AbstractValidator<GetAlertHistoryQuery>
{
    public GetAlertHistoryQueryValidator()
    {
        RuleFor(x => x.PlotId)
            .NotEmpty()
            .WithMessage("PlotId is required");

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Days must be greater than 0")
            .LessThanOrEqualTo(365)
            .WithMessage("Days cannot exceed 365");

        RuleFor(x => x.AlertType)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.AlertType))
            .WithMessage("AlertType cannot exceed 50 characters");

        RuleFor(x => x.Status)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("Status cannot exceed 20 characters");
    }
}
