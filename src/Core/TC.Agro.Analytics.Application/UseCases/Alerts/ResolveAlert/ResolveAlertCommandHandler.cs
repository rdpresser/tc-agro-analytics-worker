namespace TC.Agro.Analytics.Application.UseCases.Alerts.ResolveAlert;

/// <summary>
/// Handler for resolving alerts.
/// Sends real-time SignalR notifications after successful resolution.
/// </summary>
internal sealed class ResolveAlertCommandHandler :
    BaseCommandHandler<ResolveAlertCommand, ResolveAlertResponse, AlertAggregate, IAlertAggregateRepository>
{
    private readonly IUserContext _userContext;
    private readonly IAlertHubNotifier _alertHubNotifier;

    public ResolveAlertCommandHandler(
       IAlertAggregateRepository repository,
       IUserContext userContext,
       ITransactionalOutbox outbox,
       IAlertHubNotifier alertHubNotifier,
       ILogger<ResolveAlertCommandHandler> logger)
       : base(repository, userContext, outbox, logger)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _alertHubNotifier = alertHubNotifier ?? throw new ArgumentNullException(nameof(alertHubNotifier));
    }

    protected override async Task<Result<AlertAggregate>> MapAsync(ResolveAlertCommand command, CancellationToken ct)
    {
        var alert = await Repository.GetByIdAsync(command.AlertId, ct).ConfigureAwait(false);
        if (alert == null)
            return Result<AlertAggregate>.NotFound("Alert not found");

        var resolveResult = alert.Resolve(_userContext.Id, command.ResolutionNotes);

        if (!resolveResult.IsSuccess)
            return Result<AlertAggregate>.Invalid(resolveResult.ValidationErrors);

        return Result<AlertAggregate>.Success(alert);
    }

    protected override Task PersistAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    protected override async Task<ResolveAlertResponse> BuildResponseAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        await _alertHubNotifier.NotifyAlertResolvedAsync(
            alertId: aggregate.Id,
            sensorId: aggregate.SensorId,
            resolvedBy: aggregate.ResolvedBy!.Value,
            resolutionNotes: aggregate.ResolutionNotes,
            resolvedAt: aggregate.ResolvedAt!.Value
        ).ConfigureAwait(false);

        return ResolveAlertMapper.FromAggregate(aggregate);
    }
}
