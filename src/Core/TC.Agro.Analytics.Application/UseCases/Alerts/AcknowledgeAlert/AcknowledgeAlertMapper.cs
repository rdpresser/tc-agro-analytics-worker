namespace TC.Agro.Analytics.Application.UseCases.Alerts.AcknowledgeAlert;

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
}
