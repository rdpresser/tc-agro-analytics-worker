using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Domain.Entities;
using TC.Agro.SharedKernel.Application.Ports;

namespace TC.Agro.Analytics.Domain.Abstractions.Ports
{
    public interface ISensorReadingRepository : IBaseRepository<SensorReadingAggregate>
    {
        /// <summary>
        /// Adds an alert entity to the database context.
        /// </summary>
        Task AddAlertAsync(Alert alert, CancellationToken cancellationToken = default);
    }
}
