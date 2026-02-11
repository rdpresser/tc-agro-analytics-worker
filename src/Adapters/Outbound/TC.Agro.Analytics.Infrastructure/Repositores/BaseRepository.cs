namespace TC.Agro.Analytics.Infrastructure.Repositores;

/// <summary>
/// Base repository for Analytics service aggregates.
/// Extends SharedKernel's BaseRepository with ApplicationDbContext binding.
/// This allows concrete repositories to use BaseRepository&lt;TAggregate&gt; instead of BaseRepository&lt;TAggregate, ApplicationDbContext&gt;.
/// Following the same pattern as Identity and Farm services.
/// </summary>
public abstract class BaseRepository<TAggregate>
    : TC.Agro.SharedKernel.Infrastructure.Database.EfCore.BaseRepository<TAggregate, ApplicationDbContext>
    where TAggregate : BaseAggregateRoot
{
    protected BaseRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}
