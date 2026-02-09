namespace TC.Agro.Analytics.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Alert read model entity.
/// Alert is a read model (not an aggregate root), so it doesn't inherit from BaseEntityConfiguration.
    /// Column names are automatically converted to snake_case by UseSnakeCaseNamingConvention().
    /// </summary>
    internal sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            builder.ToTable("alerts", DefaultSchemas.Default);

            // Primary Key
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id)
                .IsRequired();

            // Foreign Keys
            builder.Property(a => a.SensorReadingId)
                .IsRequired();

            builder.Property(a => a.SensorId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.PlotId)
                .IsRequired();

            // Alert Properties
            builder.Property(a => a.AlertType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.Message)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(a => a.Status)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Pending");

            builder.Property(a => a.Severity)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Medium");

            builder.Property(a => a.Value)
                .HasColumnType("double precision");

            builder.Property(a => a.Threshold)
                .HasColumnType("double precision");

            // Audit Fields
            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(a => a.AcknowledgedAt)
                .HasColumnType("timestamptz");

            builder.Property(a => a.AcknowledgedBy)
                .HasMaxLength(100);

            builder.Property(a => a.ResolvedAt)
                .HasColumnType("timestamptz");

            builder.Property(a => a.ResolvedBy)
                .HasMaxLength(100);

            builder.Property(a => a.ResolutionNotes)
                .HasMaxLength(1000);

            // Metadata (JSON)
            builder.Property(a => a.Metadata)
                .HasColumnType("jsonb");

            // Concurrency Token
            builder.Property(a => a.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // Indexes for query performance
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

            // Composite index for common query pattern (pending alerts by plot)
            builder.HasIndex(a => new { a.PlotId, a.Status, a.CreatedAt })
                .HasDatabaseName("ix_alerts_plot_status_created");
        }
    }
