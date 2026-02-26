namespace TC.Agro.Analytics.Application.Abstractions.Ports
{
    /// <summary>
    /// Port for read-optimized sensor snapshot store operations.
    /// Handles persistence of sensor snapshots projected from external integration events.
    /// </summary>
    public interface ISensorSnapshotStore
    {
        /// <summary>
        /// Adds a new sensor snapshot to the store.
        /// </summary>
        /// <param name="snapshot">The sensor snapshot to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task AddAsync(SensorSnapshot snapshot, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing sensor snapshot in the store.
        /// </summary>
        /// <param name="snapshot">The sensor snapshot to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAsync(SensorSnapshot snapshot, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a sensor snapshot from the store by marking it as inactive.
        /// </summary>
        /// <param name="id">The sensor snapshot identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a sensor snapshot by its identifier.
        /// </summary>
        /// <param name="id">The sensor snapshot identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The sensor snapshot, or null if not found</returns>
        Task<SensorSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all active sensor snapshots for a specific plot.
        /// Used by AlertHub to resolve sensors when sending real-time notifications.
        /// </summary>
        /// <param name="plotId">The plot identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A read-only list of active sensor snapshots in the plot</returns>
        Task<IReadOnlyList<SensorSnapshot>> GetByPlotIdAsync(Guid plotId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all active sensor snapshots for a specific owner.
        /// Used by AlertHub owner-group join to preload recent alerts in owner scope.
        /// </summary>
        /// <param name="ownerId">The owner identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A read-only list of active sensor snapshots for the owner</returns>
        Task<IReadOnlyList<SensorSnapshot>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    }
}
