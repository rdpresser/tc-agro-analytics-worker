using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using TC.Agro.Analytics.Domain.Abstractions.Ports;
using TC.Agro.Analytics.Domain.Aggregates;

namespace TC.Agro.Analytics.Infrastructure.Repositores
{
    public class SensorReadingRepository : BaseRepository<SensorReadingAggregate>, ISensorReadingRepository
    {
        public SensorReadingRepository(IDocumentSession session)
            : base(session)
        {
        }

        public override async Task<IEnumerable<SensorReadingAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Marten.QueryableExtensions.ToListAsync(Session.Query<SensorReadingAggregate>(), cancellationToken);
        }
    }
}
