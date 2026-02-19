namespace TC.Agro.Analytics.Application.UseCases.Alerts.ResolveAlert;

/// <summary>
/// Handler for resolving alerts.
/// Invalidates cache after successful resolution.
/// </summary>
internal sealed class ResolveAlertCommandHandler :
    BaseCommandHandler<ResolveAlertCommand, ResolveAlertResponse, AlertAggregate, IAlertAggregateRepository>
{
    public ResolveAlertCommandHandler(
       IAlertAggregateRepository repository,
       IUserContext userContext,
       ITransactionalOutbox outbox,
       ICacheService cacheService,
       ILogger<ResolveAlertCommandHandler> logger)
       : base(repository, userContext, outbox, logger)
    {
    }

    protected override async Task<Result<AlertAggregate>> MapAsync(ResolveAlertCommand command, CancellationToken ct)
    {
        var alert = await Repository.GetByIdAsync(command.AlertId, ct).ConfigureAwait(false);

        if (alert == null)
            return Result<AlertAggregate>.NotFound("Alert not found");        

        var resolveResult = alert.Resolve(command.UserId, command.ResolutionNotes);

        if (!resolveResult.IsSuccess)
            return Result<AlertAggregate>.Invalid(resolveResult.ValidationErrors);

        return Result<AlertAggregate>.Success(alert);
    }

    protected override async Task PersistAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        Repository.Update(aggregate);
    }

    protected override Task<ResolveAlertResponse> BuildResponseAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        var response = ResolveAlertMapper.FromAggregate(aggregate);
        return Task.FromResult(response);
    }
}
