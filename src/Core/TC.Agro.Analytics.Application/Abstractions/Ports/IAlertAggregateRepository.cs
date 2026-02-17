namespace TC.Agro.Analytics.Application.Abstractions.Ports;

public interface IAlertAggregateRepository : IBaseRepository<AlertAggregate>
{
    ////Task<AlertAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ////void Add(AlertAggregate aggregate);
    void Update(AlertAggregate aggregate);
}
