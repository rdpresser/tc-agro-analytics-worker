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
            ResolvedAt: DateTimeOffset.FromFileTime(aggregate.ResolvedAt!.Value.ToFileTime()),
            ResolvedBy: aggregate.ResolvedBy!,
            ResolutionNotes: aggregate.ResolutionNotes);
    }
}
