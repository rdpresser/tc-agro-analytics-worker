using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.SharedKernel.Application.Ports;

namespace TC.Agro.Analytics.Domain.Abstractions.Ports
{
    public interface ISensorReadingRepository : IBaseRepository<SensorReadingAggregate>
    {
    }
}
