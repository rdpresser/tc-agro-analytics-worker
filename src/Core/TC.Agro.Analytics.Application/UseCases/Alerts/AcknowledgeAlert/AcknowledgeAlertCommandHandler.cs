namespace TC.Agro.Analytics.Application.UseCases.Alerts.AcknowledgeAlert;

/// <summary>
/// Handler for acknowledging alerts.
/// Inherits from BaseCommandHandler for transactional outbox pattern.
/// Sends real-time SignalR notifications after successful acknowledgment.
/// </summary>
internal sealed class AcknowledgeAlertCommandHandler :
    BaseCommandHandler<AcknowledgeAlertCommand, AcknowledgeAlertResponse, AlertAggregate, IAlertAggregateRepository>
{
    private readonly IUserContext _userContext;
    private readonly IAlertHubNotifier _alertHubNotifier;

    public AcknowledgeAlertCommandHandler(
       IAlertAggregateRepository repository,
       IUserContext userContext,
       ITransactionalOutbox outbox,
       IAlertHubNotifier alertHubNotifier,
       ILogger<AcknowledgeAlertCommandHandler> logger)
       : base(repository, userContext, outbox, logger)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _alertHubNotifier = alertHubNotifier ?? throw new ArgumentNullException(nameof(alertHubNotifier));
    }

    protected override async Task<Result<AlertAggregate>> MapAsync(AcknowledgeAlertCommand command, CancellationToken ct)
    {
        var alert = await Repository.GetByIdAsync(command.AlertId, ct).ConfigureAwait(false);

        if (alert == null)
            return Result<AlertAggregate>.NotFound("Alert not found");

        var acknowledgeResult = alert.Acknowledge(_userContext.Id);

        if (!acknowledgeResult.IsSuccess)
            return Result<AlertAggregate>.Invalid(acknowledgeResult.ValidationErrors);

        return Result<AlertAggregate>.Success(alert);
    }

    protected override Task PersistAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    protected override async Task<AcknowledgeAlertResponse> BuildResponseAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        await _alertHubNotifier.NotifyAlertAcknowledgedAsync(
            alertId: aggregate.Id,
            sensorId: aggregate.SensorId,
            acknowledgedBy: aggregate.AcknowledgedBy!.Value,
            acknowledgedAt: aggregate.AcknowledgedAt!.Value
        ).ConfigureAwait(false);

        return AcknowledgeAlertMapper.FromAggregate(aggregate);
    }
}
