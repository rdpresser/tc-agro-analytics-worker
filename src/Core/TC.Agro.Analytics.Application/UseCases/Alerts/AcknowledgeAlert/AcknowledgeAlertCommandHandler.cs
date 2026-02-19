namespace TC.Agro.Analytics.Application.UseCases.Alerts.AcknowledgeAlert;

/// <summary>
/// Handler for acknowledging alerts.
/// Inherits from BaseCommandHandler for transactional outbox pattern.
/// Invalidates cache after successful acknowledgment.
/// </summary>
internal sealed class AcknowledgeAlertCommandHandler :
    BaseCommandHandler<AcknowledgeAlertCommand, AcknowledgeAlertResponse, AlertAggregate, IAlertAggregateRepository>
{
    public AcknowledgeAlertCommandHandler(
       IAlertAggregateRepository repository,
       IUserContext userContext,
       ITransactionalOutbox outbox,
       ILogger<AcknowledgeAlertCommandHandler> logger)
       : base(repository, userContext, outbox, logger)
    {
    }

    protected override async Task<Result<AlertAggregate>> MapAsync(AcknowledgeAlertCommand command, CancellationToken ct)
    {
        var alert = await Repository.GetByIdAsync(command.AlertId, ct).ConfigureAwait(false);

        if (alert == null)
            return Result<AlertAggregate>.NotFound("Alert not found");

        var acknowledgeResult = alert.Acknowledge(command.UserId);

        if (!acknowledgeResult.IsSuccess)
            return Result<AlertAggregate>.Invalid(acknowledgeResult.ValidationErrors);

        return Result<AlertAggregate>.Success(alert);
    }

    protected override Task PersistAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    protected override Task<AcknowledgeAlertResponse> BuildResponseAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        return Task.FromResult(AcknowledgeAlertMapper.FromAggregate(aggregate));
    }
}
