namespace TC.Agro.Analytics.Infrastructure.Configurations;

public class AlertAggregateConfiguration : BaseEntityConfiguration<AlertAggregate>
{
    public override void Configure(EntityTypeBuilder<AlertAggregate> builder)
    {
        base.Configure(builder);
        builder.ToTable("alerts");

        builder.Property(e => e.SensorId)
            .IsRequired();

        builder.Property(e => e.PlotId)
            .IsRequired();

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Value)
            .IsRequired();

        builder.Property(e => e.Threshold)
            .IsRequired();

        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");

        builder.Property(e => e.AcknowledgedBy)
            .HasMaxLength(256);

        builder.Property(e => e.ResolvedBy)
            .HasMaxLength(256);

        builder.Property(e => e.ResolutionNotes)
            .HasMaxLength(1000);

        builder.Property(e => e.OwnerId)
            .IsRequired(false);

        builder.Property(e => e.Type)
            .HasConversion(
                v => v.Value,
                v => AlertType.Create(v).Value)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Severity)
            .HasConversion(
                v => v.Value,
                v => AlertSeverity.Create(v).Value)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion(
                v => v.Value,
                v => AlertStatus.Create(v).Value)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(e => e.PlotId);
        builder.HasIndex(e => e.SensorId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.PlotId, e.Status });
    }
}
