using TC.Agro.Analytics.Application.UseCases.Alerts.AcknowledgeAlert;
using TC.Agro.Analytics.Application.UseCases.Alerts.ResolveAlert;

namespace TC.Agro.Analytics.Tests.Application.Handlers;

public sealed class AlertQueriesHandlerTests
{
    [Fact]
    public void AcknowledgeAlertValidator_WithEmptyAlertId_ShouldReturnValidationError()
    {
        var validator = new AcknowledgeAlertCommandValidator();
        var command = new AcknowledgeAlertCommand(Guid.Empty);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(AcknowledgeAlertCommand.AlertId));
    }

    [Fact]
    public void AcknowledgeAlertValidator_WithValidAlertId_ShouldBeValid()
    {
        var validator = new AcknowledgeAlertCommandValidator();
        var command = new AcknowledgeAlertCommand(Guid.NewGuid());

        var result = validator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ResolveAlertValidator_WithNotesLongerThanLimit_ShouldReturnValidationError()
    {
        var validator = new ResolveAlertCommandValidator();
        var command = new ResolveAlertCommand(Guid.NewGuid(), new string('a', 1001));

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(ResolveAlertCommand.ResolutionNotes));
    }

    [Fact]
    public void ResolveAlertValidator_WithValidInput_ShouldBeValid()
    {
        var validator = new ResolveAlertCommandValidator();
        var command = new ResolveAlertCommand(Guid.NewGuid(), "Technician inspected the plot and normalized irrigation.");

        var result = validator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
