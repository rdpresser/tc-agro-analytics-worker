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
}
