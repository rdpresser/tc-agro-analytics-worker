using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TC.Agro.Analytics.Domain.Entities;
using TC.Agro.SharedKernel.Infrastructure.Database;

namespace TC.Agro.Analytics.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for Alert entity
    /// </summary>
    internal class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            // Table (using public schema from SharedKernel)
            builder.ToTable("alerts", DefaultSchemas.Default);

            // Primary Key
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(a => a.SensorReadingId)
                .HasColumnName("sensor_reading_id")
                .IsRequired();

            builder.Property(a => a.SensorId)
                .HasColumnName("sensor_id")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.PlotId)
                .HasColumnName("plot_id")
                .IsRequired();

            builder.Property(a => a.AlertType)
                .HasColumnName("alert_type")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.Message)
                .HasColumnName("message")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(a => a.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Pending");

            builder.Property(a => a.Severity)
                .HasColumnName("severity")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Medium");

            builder.Property(a => a.Value)
                .HasColumnName("value")
                .HasColumnType("double precision");

            builder.Property(a => a.Threshold)
                .HasColumnName("threshold")
                .HasColumnType("double precision");

            builder.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("now()");

            builder.Property(a => a.AcknowledgedAt)
                .HasColumnName("acknowledged_at");

            builder.Property(a => a.AcknowledgedBy)
                .HasColumnName("acknowledged_by")
                .HasMaxLength(100);

            builder.Property(a => a.ResolvedAt)
                .HasColumnName("resolved_at");

            builder.Property(a => a.ResolvedBy)
                .HasColumnName("resolved_by")
                .HasMaxLength(100);

            builder.Property(a => a.ResolutionNotes)
                .HasColumnName("resolution_notes")
                .HasMaxLength(1000);

            builder.Property(a => a.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");

            builder.Property(a => a.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion()
                .IsConcurrencyToken();

            // Indexes (performance optimization)
            builder.HasIndex(a => a.SensorReadingId)
                .HasDatabaseName("ix_alerts_sensor_reading_id");

            builder.HasIndex(a => a.SensorId)
                .HasDatabaseName("ix_alerts_sensor_id");

            builder.HasIndex(a => a.PlotId)
                .HasDatabaseName("ix_alerts_plot_id");

            builder.HasIndex(a => a.Status)
                .HasDatabaseName("ix_alerts_status");

            builder.HasIndex(a => a.AlertType)
                .HasDatabaseName("ix_alerts_type");

            builder.HasIndex(a => a.CreatedAt)
                .HasDatabaseName("ix_alerts_created_at")
                .IsDescending();

            // Composite index for common query (pending alerts by plot)
            builder.HasIndex(a => new { a.PlotId, a.Status, a.CreatedAt })
                .HasDatabaseName("ix_alerts_plot_status_created");
        }
    }
}
