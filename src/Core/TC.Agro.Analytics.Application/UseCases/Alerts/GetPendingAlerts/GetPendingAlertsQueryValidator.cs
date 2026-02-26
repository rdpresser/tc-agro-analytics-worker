namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;

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

        RuleFor(x => x.OwnerId)
            .Must(ownerId => ownerId is null || ownerId.Value != Guid.Empty)
                .WithMessage("OwnerId must be a valid non-empty GUID when provided.")
                .WithErrorCode($"{nameof(GetPendingAlertsQuery.OwnerId)}.Invalid");

        RuleFor(x => x.Severity)
            .Must(BeValidSeverity)
                .WithMessage("Severity must be one of: critical, warning, info, high, medium, low.")
                .WithErrorCode($"{nameof(GetPendingAlertsQuery.Severity)}.Invalid");

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
                .WithMessage("Status must be one of: pending, acknowledged, resolved, all.")
                .WithErrorCode($"{nameof(GetPendingAlertsQuery.Status)}.Invalid");
    }

    private static bool BeValidSeverity(string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity))
        {
            return true;
        }

        return severity.Trim().ToLowerInvariant() switch
        {
            "critical" or "warning" or "info" or "high" or "medium" or "low" => true,
            _ => false
        };
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "pending" or "acknowledged" or "resolved" or "all" => true,
            _ => false
        };
    }
}
