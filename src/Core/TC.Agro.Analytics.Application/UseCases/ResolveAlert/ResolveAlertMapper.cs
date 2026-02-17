namespace TC.Agro.Analytics.Application.UseCases.ResolveAlert;

public static class ResolveAlertMapper
{
    public static ResolveAlertResponse FromAggregate(AlertAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (aggregate.ResolvedAt == null)
        {
            throw new InvalidOperationException(
                $"Cannot map aggregate {aggregate.Id} to ResolveAlertResponse: alert is not resolved.");
        }

        return new ResolveAlertResponse(
            Id: aggregate.Id,
            Status: aggregate.Status.Value,
            ResolvedAt: DateTimeOffset.FromFileTime(aggregate.ResolvedAt.Value.ToFileTime()),
            ResolvedBy: aggregate.ResolvedBy!,
            ResolutionNotes: aggregate.ResolutionNotes);
    }
    public static AlertResolvedIntegrationEvent ToIntegrationEvent(
        AlertResolvedDomainEvent domainEvent,
        AlertAggregate aggregate)
    {
        return new AlertResolvedIntegrationEvent(
            EventId: Guid.NewGuid(),
            AlertId: domainEvent.AggregateId,
            OccurredOn: domainEvent.OccurredOn,
            EventName: nameof(AlertResolvedIntegrationEvent),
            RelatedIds: new Dictionary<string, Guid>
            {
            { "SensorId", aggregate.SensorId },
            { "PlotId", aggregate.PlotId }
            },
            SensorId: aggregate.SensorId,
            PlotId: aggregate.PlotId,
            AlertType: aggregate.Type.Value,
            ResolvedBy: domainEvent.ResolvedBy,
            ResolvedAt: domainEvent.OccurredOn,
            ResolutionNotes: domainEvent.ResolutionNotes);
    }
}
