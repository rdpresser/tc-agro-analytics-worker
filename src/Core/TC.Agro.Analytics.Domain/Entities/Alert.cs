using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TC.Agro.Analytics.Domain.Entities
{
    /// <summary>
    /// Read model entity for alerts.
    /// Projected from domain events for query optimization.
    /// This is NOT an aggregate - it's a denormalized read model for dashboard queries.
    /// </summary>
    [Table("alerts", Schema = "analytics")]
    public class Alert
    {
        /// <summary>
        /// Unique identifier for the alert
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to the sensor reading aggregate that triggered this alert
        /// </summary>
        [Column("sensor_reading_id")]
        [Required]
        public Guid SensorReadingId { get; set; }

        /// <summary>
        /// Sensor identifier
        /// </summary>
        [Column("sensor_id")]
        [Required]
        [MaxLength(100)]
        public string SensorId { get; set; } = default!;

        /// <summary>
        /// Plot identifier where the sensor is located
        /// </summary>
        [Column("plot_id")]
        [Required]
        public Guid PlotId { get; set; }

        /// <summary>
        /// Type of alert (HighTemperature, LowSoilMoisture, LowBattery)
        /// </summary>
        [Column("alert_type")]
        [Required]
        [MaxLength(50)]
        public string AlertType { get; set; } = default!;

        /// <summary>
        /// Human-readable alert message
        /// </summary>
        [Column("message")]
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = default!;

        /// <summary>
        /// Alert status (Pending, Acknowledged, Resolved)
        /// </summary>
        [Column("status")]
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Severity level (Low, Medium, High, Critical)
        /// </summary>
        [Column("severity")]
        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = "Medium";

        /// <summary>
        /// Measured value that triggered the alert
        /// </summary>
        [Column("value")]
        public double? Value { get; set; }

        /// <summary>
        /// Threshold value that was exceeded/not met
        /// </summary>
        [Column("threshold")]
        public double? Threshold { get; set; }

        /// <summary>
        /// When the alert was created
        /// </summary>
        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the alert was acknowledged
        /// </summary>
        [Column("acknowledged_at")]
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// User who acknowledged the alert
        /// </summary>
        [Column("acknowledged_by")]
        [MaxLength(100)]
        public string? AcknowledgedBy { get; set; }

        /// <summary>
        /// When the alert was resolved
        /// </summary>
        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// User who resolved the alert
        /// </summary>
        [Column("resolved_by")]
        [MaxLength(100)]
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// Resolution notes
        /// </summary>
        [Column("resolution_notes")]
        [MaxLength(1000)]
        public string? ResolutionNotes { get; set; }

        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        [Column("metadata", TypeName = "jsonb")]
        public string? Metadata { get; set; }

        /// <summary>
        /// For optimistic concurrency control
        /// </summary>
        [Timestamp]
        [Column("row_version")]
        public byte[]? RowVersion { get; set; }
    }

    /// <summary>
    /// Alert types enum (for type safety)
    /// </summary>
    public static class AlertTypes
    {
        public const string HighTemperature = "HighTemperature";
        public const string LowSoilMoisture = "LowSoilMoisture";
        public const string LowBattery = "LowBattery";
    }

    /// <summary>
    /// Alert status enum
    /// </summary>
    public static class AlertStatus
    {
        public const string Pending = "Pending";
        public const string Acknowledged = "Acknowledged";
        public const string Resolved = "Resolved";
    }

    /// <summary>
    /// Alert severity levels
    /// </summary>
    public static class AlertSeverity
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Critical = "Critical";
    }
}
