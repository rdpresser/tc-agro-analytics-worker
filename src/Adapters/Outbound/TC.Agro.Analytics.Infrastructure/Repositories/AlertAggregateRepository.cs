namespace TC.Agro.Analytics.Infrastructure.Repositories;

public class AlertAggregateRepository : BaseRepository<AlertAggregate>, IAlertAggregateRepository
{
    public AlertAggregateRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    /// <summary>
    /// Loads UserAggregate with owned entities (Email, Role) explicitly included.
    /// FindAsync does not guarantee eager loading of owned entities, which causes
    /// null constraint violations on SaveChanges when only non-owned properties are modified.
    /// </summary>
    public override async Task<AlertAggregate?> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Id == aggregateId, cancellationToken)
            .ConfigureAwait(false);
    }
}
