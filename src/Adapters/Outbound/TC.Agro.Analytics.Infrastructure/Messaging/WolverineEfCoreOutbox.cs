using Microsoft.EntityFrameworkCore;
using TC.Agro.SharedKernel.Application.Ports;
using Wolverine.EntityFrameworkCore;

namespace TC.Agro.Analytics.Infrastructure.Messaging;

/// <summary>
/// Wolverine-based implementation of ITransactionalOutbox for EF Core.
/// Provides atomic EF Core persistence and message publishing via Outbox Pattern.
/// </summary>
public class WolverineEfCoreOutbox : ITransactionalOutbox
{
    private readonly IDbContextOutbox<ApplicationDbContext> _outbox;

    public WolverineEfCoreOutbox(IDbContextOutbox<ApplicationDbContext> outbox)
    {
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
    }

    public ValueTask EnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _outbox.PublishAsync(message);
    }

    public ValueTask EnqueueAsync<T>(IReadOnlyCollection<T> messages, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _outbox.PublishAsync(messages.ToArray());
    }

    /// <summary>
    /// Commits EF Core changes and flushes Wolverine outbox messages in a single transaction.
    /// This ensures atomicity between data persistence and message publishing.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await _outbox.SaveChangesAndFlushMessagesAsync(ct).ConfigureAwait(false);
        return 1;
    }
}
