namespace TC.Agro.Analytics.Infrastructure.Configurations
{

    [ExcludeFromCodeCoverage]
    public class Configuration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseAggregateRoot
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamptz")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false)
                .HasColumnType("timestamptz");
        }
    }
}
