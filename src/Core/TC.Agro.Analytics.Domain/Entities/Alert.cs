namespace TC.Agro.Analytics.Domain.Entities;

/// <summary>
/// Read model entity for alerts - database projection (CQRS pattern).
/// While typically read models live in Infrastructure, this stays in Domain because:
/// 1. SensorReadingAggregate creates it from domain events
/// 2. It's a projection OF domain events (not infrastructure concern)
/// 3. Identity Service does this differently (projects Aggregate → Response in ReadStore)
/// 
/// This is a valid CQRS pattern variation where the read model is domain-driven.
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
