namespace TC.Agro.Analytics.Application.UseCases.Alerts.ResolveAlert;

public static class ResolveAlertMapper
{
    /// <summary>
    /// Maps AlertAggregate to ResolveAlertResponse.
    /// Called after successfully resolving an alert.
    /// Assumes aggregate is already validated (no null checks or business validation).
    /// </summary>
    public static ResolveAlertResponse FromAggregate(AlertAggregate aggregate)
    {
        return new ResolveAlertResponse(
            Id: aggregate.Id,
            Status: aggregate.Status.Value,
            ResolvedAt: new DateTimeOffset(aggregate.ResolvedAt!.Value, TimeSpan.Zero),
            ResolvedBy: aggregate.ResolvedBy!,
            ResolutionNotes: aggregate.ResolutionNotes);
    }
}
