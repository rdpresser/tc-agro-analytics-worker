namespace TC.Agro.Analytics.Application.Abstractions.Ports;

/// <summary>
/// Repository interface for AlertAggregate.
/// Following Farm/Identity pattern: interface in Application.Abstractions.Ports.
/// Farm: IPlotAggregateRepository, IPropertyAggregateRepository
/// Identity: IUserAggregateRepository
/// Analytics: IAlertAggregateRepository
/// </summary>
public interface IAlertAggregateRepository
{
    Task<AlertAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(AlertAggregate aggregate);
    void Update(AlertAggregate aggregate);
}
