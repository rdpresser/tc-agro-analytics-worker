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
    /// Assumes aggregate is already validated (no null checks or business validation).
    /// </summary>
    public static AcknowledgeAlertResponse FromAggregate(AlertAggregate aggregate)
    {

        return new AcknowledgeAlertResponse(
            Id: aggregate.Id,
            Status: aggregate.Status.Value,
            AcknowledgedAt: DateTimeOffset.FromFileTime(aggregate.AcknowledgedAt!.Value.ToFileTime()),
            AcknowledgedBy: aggregate.AcknowledgedBy!);
    }
}
