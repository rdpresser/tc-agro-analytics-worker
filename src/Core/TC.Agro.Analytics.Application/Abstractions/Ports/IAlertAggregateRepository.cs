namespace TC.Agro.Analytics.Application.Abstractions.Ports;

public interface IAlertAggregateRepository : IBaseRepository<AlertAggregate>
{
    void Update(AlertAggregate aggregate);
}
