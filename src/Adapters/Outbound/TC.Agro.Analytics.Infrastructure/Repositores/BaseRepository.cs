namespace TC.Agro.Analytics.Infrastructure.Repositores
{
    public abstract class BaseRepository<TAggregate> : IBaseRepository<TAggregate>, IDisposable, IAsyncDisposable
       where TAggregate : BaseAggregateRoot
    {
        private bool _disposed;
        protected ApplicationDbContext DbContext { get; }
        protected DbSet<TAggregate> DbSet { get; }

        protected BaseRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbSet = dbContext.Set<TAggregate>();
        }

        public void Add(TAggregate aggregate)
        {
            DbSet.Add(aggregate);
        }

        public async Task DeleteAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            var aggregateRoot = await GetByIdAsync(aggregateId, cancellationToken)
                .ConfigureAwait(false);

            if (aggregateRoot != null)
            {
                Delete(aggregateRoot);
            }
        }

        public void Delete(TAggregate aggregateRoot)
        {
            DbSet.Remove(aggregateRoot);
        }

        public Task<IEnumerable<TAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregate?> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DbContext.Dispose();
                }

                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            // Suppress finalization to prevent the finalizer from running
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                if (DbContext != null)
                {
                    await DbContext.DisposeAsync().ConfigureAwait(false);
                }

                _disposed = true;
            }
        }
    }
}
