namespace TC.Agro.Analytics.Application.UseCases.ResolveAlert
{
    public sealed record ResolveAlertResponse(
        Guid Id,
        string Status,
        DateTimeOffset ResolvedAt,
        string ResolvedBy,
        string? ResolutionNotes,
        string Message = "Alert resolved successfully");
}
