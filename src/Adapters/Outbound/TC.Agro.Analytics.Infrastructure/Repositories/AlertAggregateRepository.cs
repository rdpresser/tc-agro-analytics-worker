namespace TC.Agro.Analytics.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for AlertAggregate.
/// Following Farm/Identity pattern: {Nome}AggregateRepository extending BaseRepository.
/// </summary>
public class AlertAggregateRepository : BaseRepository<AlertAggregate>, IAlertAggregateRepository
{
    public AlertAggregateRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public void Update(AlertAggregate aggregate)
    {
        DbContext.Set<AlertAggregate>().Update(aggregate);
    }
}
