namespace TC.Agro.Analytics.Domain.Entities
{
    /// <summary>
    /// Read model entity for alerts.
    /// Projected from domain events for query optimization.
    /// This is NOT an aggregate - it's a denormalized read model for dashboard queries.
    /// Configuration is done via EntityTypeConfiguration in Infrastructure layer.
    /// </summary>
    public class Alert
    {
        public Guid Id { get; set; }
        public Guid SensorReadingId { get; set; }
        public string SensorId { get; set; } = default!;
        public Guid PlotId { get; set; }
        public string AlertType { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Status { get; set; } = "Pending";
        public string Severity { get; set; } = "Medium";
        public double? Value { get; set; }
        public double? Threshold { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? Metadata { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}
