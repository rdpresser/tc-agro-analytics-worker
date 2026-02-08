using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.SharedKernel.Infrastructure.Database;
using TC.Agro.SharedKernel.Infrastructure.Database.EfCore;

namespace TC.Agro.Analytics.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for SensorReadingAggregate.
/// Inherits base configuration from BaseEntityConfiguration (Id, CreatedAt, UpdatedAt, IsActive).
/// Column names are automatically converted to snake_case by UseSnakeCaseNamingConvention().
/// </summary>
internal sealed class SensorReadingAggregateConfiguration : BaseEntityConfiguration<SensorReadingAggregate>
{
    public override void Configure(EntityTypeBuilder<SensorReadingAggregate> builder)
    {
        // Configure base properties (Id, CreatedAt, UpdatedAt, IsActive, UncommittedEvents)
        base.Configure(builder);

        builder.ToTable("sensor_readings", DefaultSchemas.Default);

        builder.Property(x => x.SensorId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PlotId)
            .IsRequired();

        builder.Property(x => x.Time)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.Temperature);

        builder.Property(x => x.Humidity);

        builder.Property(x => x.SoilMoisture);

        builder.Property(x => x.Rainfall);

        builder.Property(x => x.BatteryLevel);

        // Indexes for queries
        builder.HasIndex(x => x.SensorId)
            .HasDatabaseName("ix_sensor_readings_sensor_id");

        builder.HasIndex(x => x.PlotId)
            .HasDatabaseName("ix_sensor_readings_plot_id");

        builder.HasIndex(x => x.Time)
            .HasDatabaseName("ix_sensor_readings_time");
    }
}
