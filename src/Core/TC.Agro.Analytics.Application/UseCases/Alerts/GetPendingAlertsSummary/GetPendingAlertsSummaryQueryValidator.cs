namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;

public sealed class GetPendingAlertsSummaryQueryValidator : AbstractValidator<GetPendingAlertsSummaryQuery>
{
    public GetPendingAlertsSummaryQueryValidator()
    {
        RuleFor(x => x.OwnerId)
            .Must(ownerId => ownerId is null || ownerId.Value != Guid.Empty)
            .WithMessage("OwnerId must be a valid non-empty GUID when provided.")
            .WithErrorCode($"{nameof(GetPendingAlertsSummaryQuery.OwnerId)}.Invalid");

        RuleFor(x => x.WindowHours)
            .GreaterThan(0)
            .WithMessage("WindowHours must be greater than 0.")
            .WithErrorCode($"{nameof(GetPendingAlertsSummaryQuery.WindowHours)}.GreaterThanZero")
            .LessThanOrEqualTo(720)
            .WithMessage("WindowHours cannot exceed 720 hours (30 days).")
            .WithErrorCode($"{nameof(GetPendingAlertsSummaryQuery.WindowHours)}.MaximumValue");
    }
}
