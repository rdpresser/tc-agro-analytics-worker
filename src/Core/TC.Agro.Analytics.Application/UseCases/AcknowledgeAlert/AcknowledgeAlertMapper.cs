namespace TC.Agro.Analytics.Application.UseCases.AcknowledgeAlert;

/// <summary>
/// Mapper for AcknowledgeAlert use case.
/// Maps between AlertAggregate and AcknowledgeAlertResponse.
/// </summary>
public static class AcknowledgeAlertMapper
{
    /// <summary>
    /// Maps AlertAggregate to AcknowledgeAlertResponse.
    /// Called after successfully acknowledging an alert.
    /// </summary>
    public static AcknowledgeAlertResponse FromAggregate(AlertAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (aggregate.AcknowledgedAt == null)
        {
            throw new InvalidOperationException(
                $"Cannot map aggregate {aggregate.Id} to AcknowledgeAlertResponse: alert is not acknowledged.");
        }

        return new AcknowledgeAlertResponse(
            Id: aggregate.Id,
            Status: aggregate.Status.Value,
            AcknowledgedAt: DateTimeOffset.FromFileTime(aggregate.AcknowledgedAt.Value.ToFileTime()),
            AcknowledgedBy: aggregate.AcknowledgedBy!);
    }

    /// <summary>
    /// Maps AlertAcknowledgedDomainEvent to AlertAcknowledgedIntegrationEvent.
    /// Used for publishing to message broker via Outbox pattern.
    /// </summary>
    public static AlertAcknowledgedIntegrationEvent ToIntegrationEvent(
        AlertAcknowledgedDomainEvent domainEvent,
        AlertAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        ArgumentNullException.ThrowIfNull(aggregate);

        return new AlertAcknowledgedIntegrationEvent(
            EventId: Guid.NewGuid(),
            AlertId: domainEvent.AggregateId,
            OccurredOn: domainEvent.OccurredOn,
            EventName: nameof(AlertAcknowledgedIntegrationEvent),
            RelatedIds: new Dictionary<string, Guid>
            {
                { "SensorId", aggregate.SensorId },
                { "PlotId", aggregate.PlotId }
            },
            SensorId: aggregate.SensorId,
            PlotId: aggregate.PlotId,
            AlertType: aggregate.Type.Value,
            AcknowledgedBy: domainEvent.AcknowledgedBy,
            AcknowledgedAt: domainEvent.OccurredOn);
    }
}
