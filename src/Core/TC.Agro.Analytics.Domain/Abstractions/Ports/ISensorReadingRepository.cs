namespace TC.Agro.Analytics.Domain.Abstractions.Ports;

public interface ISensorReadingRepository : IBaseRepository<SensorReadingAggregate>
{
    /// <summary>
    /// Adds an alert entity to the database context.
    /// </summary>
    Task AddAlertAsync(Alert alert, CancellationToken cancellationToken = default);
}
