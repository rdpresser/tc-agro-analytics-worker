namespace TC.Agro.Analytics.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for SensorSnapshot.
/// Follows OwnerSnapshotStore pattern.
/// </summary>
public sealed class SensorSnapshotStore : ISensorSnapshotStore
{
    private readonly ApplicationDbContext _dbContext;

    public SensorSnapshotStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<SensorSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensorSnapshots
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
                .ConfigureAwait(false);
    }

    public async Task AddAsync(SensorSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        await _dbContext.SensorSnapshots.AddAsync(snapshot, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var snapshot = await _dbContext.SensorSnapshots
               .IgnoreQueryFilters() // Allows finding inactive records for soft-delete
               .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
               .ConfigureAwait(false);

        if (snapshot == null)
            return;

        snapshot.Delete();
        _dbContext.SensorSnapshots.Update(snapshot);
    }

    public async Task<IReadOnlyList<SensorSnapshot>> GetByPlotIdAsync(Guid plotId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensorSnapshots
            .AsNoTracking()
            .Where(s => s.PlotId == plotId && s.IsActive)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SensorSnapshot>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensorSnapshots
            .AsNoTracking()
            .Where(s => s.OwnerId == ownerId && s.IsActive)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
