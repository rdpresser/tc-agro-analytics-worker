using TC.Agro.Analytics.Application.UseCases.Shared;

namespace TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Validator for GetPendingAlertsQuery.
/// </summary>
public sealed class GetPendingAlertsQueryValidator : AbstractValidator<GetPendingAlertsQuery>
{
    public GetPendingAlertsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.")
                .WithErrorCode($"{nameof(GetPendingAlertsQuery.PageNumber)}.GreaterThanZero");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
                .WithMessage("Page size must be greater than 0.")
                .WithErrorCode($"{nameof(GetPendingAlertsQuery.PageSize)}.GreaterThanZero")
            .LessThanOrEqualTo(PaginationParams.MaxPageSize)
                .WithMessage($"Page size cannot exceed {PaginationParams.MaxPageSize}.")
                .WithErrorCode($"{nameof(GetPendingAlertsQuery.PageSize)}.MaximumValue");
    }
}
