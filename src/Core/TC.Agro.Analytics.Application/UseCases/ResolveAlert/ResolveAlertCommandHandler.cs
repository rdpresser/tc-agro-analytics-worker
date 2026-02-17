namespace TC.Agro.Analytics.Application.UseCases.ResolveAlert;

internal sealed class ResolveAlertCommandHandler :
    BaseCommandHandler<ResolveAlertCommand, ResolveAlertResponse, AlertAggregate, IAlertAggregateRepository>
{
    public ResolveAlertCommandHandler(
       IAlertAggregateRepository repository,
       IUserContext userContext,
       ITransactionalOutbox outbox,
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

    protected override Task PersistAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        Repository.Update(aggregate);
        return Task.CompletedTask;
    }

    protected override Task<ResolveAlertResponse> BuildResponseAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        var response = ResolveAlertMapper.FromAggregate(aggregate);
        return Task.FromResult(response);
    }

    protected override async Task PublishIntegrationEventsAsync(AlertAggregate aggregate, CancellationToken ct)
    {
        var integrationEvents = aggregate.UncommittedEvents
            .MapToIntegrationEvents(
                aggregate: aggregate,
                userContext: UserContext,
                handlerName: nameof(ResolveAlertCommandHandler),
                mappings: new Dictionary<Type, Func<BaseDomainEvent, BaseIntegrationEvent>>
                {
                { typeof(AlertResolvedDomainEvent), e => ResolveAlertMapper.ToIntegrationEvent((AlertResolvedDomainEvent)e, aggregate) }
                });

        foreach (var evt in integrationEvents)
        {
            await Outbox.EnqueueAsync(evt, ct).ConfigureAwait(false);
        }
    }
}
