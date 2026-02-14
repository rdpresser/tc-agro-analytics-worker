namespace TC.Agro.Analytics.Application.UseCases.GetAlertHistory;

/// <summary>
/// Validator for GetAlertHistoryQuery.
/// </summary>
public sealed class GetAlertHistoryQueryValidator : AbstractValidator<GetAlertHistoryQuery>
{
    public GetAlertHistoryQueryValidator()
    {
        RuleFor(x => x.PlotId)
            .NotEmpty()
                .WithMessage("PlotId is required.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.PlotId)}.Required");

        RuleFor(x => x.Days)
            .GreaterThan(0)
                .WithMessage("Days must be greater than 0.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.Days)}.GreaterThanZero")
            .LessThanOrEqualTo(365)
                .WithMessage("Days cannot exceed 365.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.Days)}.MaximumValue");

        RuleFor(x => x.AlertType)
            .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.AlertType))
                .WithMessage("AlertType cannot exceed 50 characters.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.AlertType)}.MaximumLength");

        RuleFor(x => x.Status)
            .MaximumLength(20)
                .When(x => !string.IsNullOrEmpty(x.Status))
                .WithMessage("Status cannot exceed 20 characters.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.Status)}.MaximumLength");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.PageNumber)}.GreaterThanZero");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
                .WithMessage("Page size must be greater than 0.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.PageSize)}.GreaterThanZero")
            .LessThanOrEqualTo(PaginationParams.MaxPageSize)
                .WithMessage($"Page size cannot exceed {PaginationParams.MaxPageSize}.")
                .WithErrorCode($"{nameof(GetAlertHistoryQuery.PageSize)}.MaximumValue");
    }
}
