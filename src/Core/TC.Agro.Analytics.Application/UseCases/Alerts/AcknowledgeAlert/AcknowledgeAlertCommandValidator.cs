namespace TC.Agro.Analytics.Application.UseCases.Alerts.AcknowledgeAlert;

/// <summary>
/// Validator for AcknowledgeAlert command.
/// </summary>
public sealed class AcknowledgeAlertCommandValidator : Validator<AcknowledgeAlertCommand>
{
    public AcknowledgeAlertCommandValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty()
            .WithMessage("AlertId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required for audit trail.");
    }
}
