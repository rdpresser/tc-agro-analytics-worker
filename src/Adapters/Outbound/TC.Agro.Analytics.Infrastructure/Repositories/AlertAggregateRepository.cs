namespace TC.Agro.Analytics.Infrastructure.Repositories;

public class AlertAggregateRepository : BaseRepository<AlertAggregate>, IAlertAggregateRepository
{
    public AlertAggregateRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public void Update(AlertAggregate aggregate)
    {
        DbContext.Set<AlertAggregate>().Update(aggregate);
    }
}
