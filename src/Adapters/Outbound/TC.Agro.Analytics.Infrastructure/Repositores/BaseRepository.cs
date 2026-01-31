using Marten;

namespace TC.Agro.Analytics.Infrastructure.Repositores
{
    public abstract class BaseRepository<TAggregate> : IBaseRepository<TAggregate>
       where TAggregate : BaseAggregateRoot
    {
        private readonly IDocumentSession _session;

        protected BaseRepository(IDocumentSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected IDocumentSession Session => _session ?? throw new InvalidOperationException("Document session is not initialized.");

        /// <summary>
        /// Retrieves an aggregate by its ID using snapshot (no replay of events).prewsidente ja
        /// Returns null if not found.
        /// </summary>
        ////public async Task<TAggregate?> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        ////    => await Session.Events.AggregateStreamAsync<TAggregate>(aggregateId, token: cancellationToken).ConfigureAwait(false);

        public async Task<TAggregate?> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
            => await Session.LoadAsync<TAggregate>(aggregateId, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Adds the aggregate state (including new domain events if any).
        /// For event-sourced aggregates, this starts a new event stream.
        /// </summary>
        public void Add(TAggregate aggregate)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            var uncommitted = aggregate.UncommittedEvents?.ToArray();
            if (uncommitted is { Length: > 0 })
            {
                Session.Events.StartStream<TAggregate>(aggregate.Id, uncommitted);
            }
            else
            {
                IEnumerable<TAggregate> aggregates = new[] { aggregate };
                Session.Store(aggregates);
            }
        }

        /// <summary>
        /// Retrieves all aggregates. Implementation depends on concrete repository.
        /// </summary>
        public abstract Task<IEnumerable<TAggregate>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes the aggregate identified by its ID.
        /// </summary>
        public async Task DeleteAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            var aggregate = await GetByIdAsync(aggregateId, cancellationToken).ConfigureAwait(false);
            if (aggregate != null)
            {
                Session.Delete(aggregate);
                await Session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Inserts or appends uncommitted events into the event stream.
        /// Does NOT call SaveChanges.
        /// </summary>
        protected async Task InsertOrUpdateAsync(Guid aggregateId, CancellationToken cancellationToken = default, params object[] events)
        {
            if (events == null || events.Length == 0)
                return;

            var existingAggregate = await GetByIdAsync(aggregateId, cancellationToken).ConfigureAwait(false);

            if (existingAggregate == null)
                Session.Events.StartStream<TAggregate>(aggregateId, events);
            else
                Session.Events.Append(aggregateId, events);
        }

        /// <summary>
        /// Saves uncommitted events of the aggregate into the event stream.
        /// Does NOT commit the session.
        /// </summary>
        public async Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            var uncommitted = aggregate.UncommittedEvents?.ToArray();
            if (uncommitted is { Length: > 0 })
            {
                await InsertOrUpdateAsync(aggregate.Id, cancellationToken, uncommitted).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Commits the session, persisting all changes (events/documents) to the database.
        /// Marks aggregate events as committed.
        /// This guarantees transactional outbox with Wolverine.
        /// </summary>
        public async Task CommitAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
        {
            await Session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            aggregate.MarkEventsAsCommitted();
        }

        /// <summary>
        /// Convenience method to save + commit in a single call.
        /// </summary>
        public async Task PersistAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
        {
            await SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);
            await CommitAsync(aggregate, cancellationToken).ConfigureAwait(false);
        }
    }
}
