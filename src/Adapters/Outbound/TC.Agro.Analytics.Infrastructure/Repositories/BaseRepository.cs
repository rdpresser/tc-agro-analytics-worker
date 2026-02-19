namespace TC.Agro.Analytics.Infrastructure.Repositories;

/// <summary>
/// Base repository for Analytics service aggregates.
/// Extends SharedKernel's BaseRepository with ApplicationDbContext binding.
/// This allows concrete repositories to use BaseRepository&lt;TAggregate&gt; instead of BaseRepository&lt;TAggregate, ApplicationDbContext&gt;.
/// Following the same pattern as Identity and Analytics services.
/// </summary>
public abstract class BaseRepository<TAggregate>(ApplicationDbContext dbContext)
    : BaseRepository<TAggregate, ApplicationDbContext>(dbContext)
    where TAggregate : BaseAggregateRoot
{
}
