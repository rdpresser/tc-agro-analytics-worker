namespace TC.Agro.Analytics.Application.Abstractions.Ports
{
    public interface IUnitOfWork
    {

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
