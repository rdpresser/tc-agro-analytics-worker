namespace TC.Agro.Analytics.Infrastructure.Messaging;

/// <summary>
/// Analytics service specific Outbox binding.
/// Uses SharedKernel's generic WolverineEfCoreOutbox with ApplicationDbContext.
/// </summary>
public sealed class AnalyticsOutbox : WolverineEfCoreOutbox<ApplicationDbContext>
{
    public AnalyticsOutbox(IDbContextOutbox<ApplicationDbContext> outbox) : base(outbox) { }
}
