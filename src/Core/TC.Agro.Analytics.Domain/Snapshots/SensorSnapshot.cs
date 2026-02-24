namespace TC.Agro.Analytics.Domain.Snapshots
{
    public sealed class SensorSnapshot
    {
        public Guid Id { get; private set; } // SensorId
        public Guid OwnerId { get; private set; }
        public OwnerSnapshot Owner { get; private set; } = default!;
        public Guid PropertyId { get; private set; }
        public Guid PlotId { get; private set; }

        public string? Label { get; private set; }
        public string PlotName { get; private set; } = default!;
        public string PropertyName { get; private set; } = default!;
        public string? Status { get; private set; }
        public bool IsActive { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        public ICollection<AlertAggregate> Alerts { get; private set; } = [];

        private SensorSnapshot() { } // EF

        private SensorSnapshot(
            Guid id,
            Guid ownerId,
            Guid propertyId,
            Guid plotId,
            string? label,
            string plotName,
            string propertyName,
            bool isActive,
            DateTimeOffset createdAt,
            DateTimeOffset? updatedAt,
            string? status = null)
        {
            Id = id;
            OwnerId = ownerId;
            PropertyId = propertyId;
            PlotId = plotId;
            Label = label;
            PlotName = plotName;
            PropertyName = propertyName;
            IsActive = isActive;
            CreatedAt = createdAt;
            Status = status;
            UpdatedAt = updatedAt;
        }

        // Factory used when SensorRegistered event arrives
        public static SensorSnapshot Create(
            Guid id,
            Guid ownerId,
            Guid propertyId,
            Guid plotId,
            string? label,
            string plotName,
            string propertyName,
            string? status = null
            )
        {
            var now = DateTimeOffset.UtcNow;

            return new SensorSnapshot(
                id,
                ownerId,
                propertyId,
                plotId,
                label,
                plotName,
                propertyName,
                true,
                now,
                null,
                status
                );
        }

        // Factory when event already includes createdAt
        public static SensorSnapshot Create(
            Guid id,
            Guid ownerId,
            Guid propertyId,
            Guid plotId,
            string? label,
            string plotName,
            string propertyName,
            DateTimeOffset createdAt,
            string? status = null)
        {
            return new SensorSnapshot(
                id,
                ownerId,
                propertyId,
                plotId,
                label,
                plotName,
                propertyName,
                true,
                createdAt,
                null,
                status);
        }

        // Update when SensorUpdated or PlotUpdated event arrives
        public void Update(
            Guid ownerId,
            Guid propertyId,
            Guid plotId,
            string sensorName,
            string plotName,
            string propertyName,
            string status)
        {
            OwnerId = ownerId;
            PropertyId = propertyId;
            PlotId = plotId;
            Status = status;
            Label = sensorName;
            PlotName = plotName;
            PropertyName = propertyName;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Soft delete - marks sensor as inactive when removed
        public void Delete()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
