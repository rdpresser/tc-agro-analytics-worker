using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.SharedKernel.Infrastructure.Database;

namespace TC.Agro.Analytics.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for SensorReadingAggregate.
/// Maps the aggregate as a document (JSON) for simple persistence.
/// </summary>
public class SensorReadingAggregateConfiguration : IEntityTypeConfiguration<SensorReadingAggregate>
{
    public void Configure(EntityTypeBuilder<SensorReadingAggregate> builder)
    {
        builder.ToTable("sensor_readings", DefaultSchemas.Default);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.SensorId)
            .HasColumnName("sensor_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PlotId)
            .HasColumnName("plot_id")
            .IsRequired();

        builder.Property(x => x.Time)
            .HasColumnName("time")
            .IsRequired();

        builder.Property(x => x.Temperature)
            .HasColumnName("temperature")
            .IsRequired();

        builder.Property(x => x.Humidity)
            .HasColumnName("humidity")
            .IsRequired();

        builder.Property(x => x.SoilMoisture)
            .HasColumnName("soil_moisture")
            .IsRequired();

        builder.Property(x => x.Rainfall)
            .HasColumnName("rainfall")
            .IsRequired();

        builder.Property(x => x.BatteryLevel)
            .HasColumnName("battery_level")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        // Ignore domain events (not persisted)
        builder.Ignore(x => x.UncommittedEvents);

        // Index for queries
        builder.HasIndex(x => x.SensorId)
            .HasDatabaseName("ix_sensor_readings_sensor_id");

        builder.HasIndex(x => x.PlotId)
            .HasDatabaseName("ix_sensor_readings_plot_id");

        builder.HasIndex(x => x.Time)
            .HasDatabaseName("ix_sensor_readings_time");
    }
}
