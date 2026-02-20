namespace TC.Agro.Analytics.Application.UseCases.Alerts.ResolveAlert
{
    public sealed record ResolveAlertCommand(
        Guid AlertId,
        string? ResolutionNotes
    ) : IBaseCommand<ResolveAlertResponse>, IInvalidateCache
    {
        public IReadOnlyCollection<string> CacheTags =>
        [
            CacheTagCatalog.Alerts,
            CacheTagCatalog.AlertHistory,
            CacheTagCatalog.PlotStatus
        ];
    }
}
