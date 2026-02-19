namespace TC.Agro.Analytics.Infrastructure.Messaging
{
    public sealed class AnalyticsOutbox : WolverineEfCoreOutbox<ApplicationDbContext>
    {
        public AnalyticsOutbox(IDbContextOutbox<ApplicationDbContext> outbox) : base(outbox) { }
    }
}
