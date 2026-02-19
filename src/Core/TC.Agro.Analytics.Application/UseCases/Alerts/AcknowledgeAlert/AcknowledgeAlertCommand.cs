namespace TC.Agro.Analytics.Application.UseCases.Alerts.AcknowledgeAlert;

/// <summary>
/// Command for acknowledging an alert.
/// Business rule: User must be identified for audit trail.
/// </summary>
public sealed record AcknowledgeAlertCommand(
    Guid AlertId,
    string UserId) : IBaseCommand<AcknowledgeAlertResponse>, IInvalidateCache
{
    public IReadOnlyCollection<string> CacheTags =>
    [
        CacheTagCatalog.Alerts,
        CacheTagCatalog.AlertHistory,
        CacheTagCatalog.PlotStatus
    ];
}
