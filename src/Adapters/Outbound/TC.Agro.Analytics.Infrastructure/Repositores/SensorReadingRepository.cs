using TC.Agro.Analytics.Domain.Abstractions.Ports;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Domain.Entities;
using TC.Agro.SharedKernel.Infrastructure.Database.EfCore;

namespace TC.Agro.Analytics.Infrastructure.Repositores;

/// <summary>
/// EF Core repository for SensorReadingAggregate.
/// Follows the same pattern as Identity-Service (EF Core, not Marten).
/// </summary>
public class SensorReadingRepository 
    : BaseRepository<SensorReadingAggregate, ApplicationDbContext>, 
      ISensorReadingRepository
{
    public SensorReadingRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task AddAlertAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        await DbContext.Alerts.AddAsync(alert, cancellationToken);
    }
}
