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

    public async Task UpdateAsync(SensorSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var existingSnapshot = await _dbContext.SensorSnapshots
            .IgnoreQueryFilters() // ✅ Permite encontrar/atualizar registros inativos
            .FirstOrDefaultAsync(o => o.Id == snapshot.Id, cancellationToken)
            .ConfigureAwait(false);

        if (existingSnapshot == null)
            return;

        // ✅ Atualiza a instância rastreada (evita conflito de tracking)
        _dbContext.Entry(existingSnapshot).CurrentValues.SetValues(snapshot);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var snapshot = await _dbContext.SensorSnapshots
               .IgnoreQueryFilters() // ✅ Permite encontrar registros inativos para soft-delete
               .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
               .ConfigureAwait(false);

        if (snapshot == null)
            return;

        snapshot.Delete();
        _dbContext.SensorSnapshots.Update(snapshot);
    }
}
