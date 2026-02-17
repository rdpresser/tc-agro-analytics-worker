namespace TC.Agro.Analytics.Application.UseCases.ResolveAlert
{
    public sealed class ResolveAlertCommandValidator : Validator<ResolveAlertCommand>
    {
        public ResolveAlertCommandValidator()
        {
            RuleFor(x => x.AlertId)
                .NotEmpty()
                .WithMessage("AlertId is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required for audit trail.");

            RuleFor(x => x.ResolutionNotes)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.ResolutionNotes))
                .WithMessage("Resolution notes must not exceed 1000 characters.");
        }
    }
}
