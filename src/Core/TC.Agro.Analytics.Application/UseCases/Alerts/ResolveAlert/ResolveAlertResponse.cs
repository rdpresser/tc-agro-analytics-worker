namespace TC.Agro.Analytics.Application.UseCases.Alerts.ResolveAlert
{
    public sealed record ResolveAlertResponse(
        Guid Id,
        string Status,
        DateTimeOffset ResolvedAt,
        Guid ResolvedBy,
        string? ResolutionNotes,
        string Message = "Alert resolved successfully");
}
