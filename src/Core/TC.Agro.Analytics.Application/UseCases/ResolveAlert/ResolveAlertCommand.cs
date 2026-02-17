namespace TC.Agro.Analytics.Application.UseCases.ResolveAlert
{
    public sealed record ResolveAlertCommand(
        Guid AlertId,
        string UserId,
        string? ResolutionNotes
    ) : IBaseCommand<ResolveAlertResponse>;
}
