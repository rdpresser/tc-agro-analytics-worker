namespace TC.Agro.Analytics.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    DbContext IApplicationDbContext.DbContext => this;
    public DbSet<AlertAggregate> Alerts { get; set; } = default!;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(DefaultSchemas.Default);

        // Configure all DateTime properties to use UTC (PostgreSQL timestamptz requirement)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new ValueConverter<DateTime, DateTime>(
                            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }
    }

    async Task<int> SharedKernel.Application.Ports.IUnitOfWork.SaveChangesAsync(CancellationToken ct)
    {
        Log.Debug("ApplicationDbContext.SaveChangesAsync called. ChangeTracker has {Count} entries",
            ChangeTracker.Entries().Count());

        var entriesBeforeSave = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        Log.Debug("Entries to save: Added={Added}, Modified={Modified}, Deleted={Deleted}",
            entriesBeforeSave.Count(e => e.State == EntityState.Added),
            entriesBeforeSave.Count(e => e.State == EntityState.Modified),
            entriesBeforeSave.Count(e => e.State == EntityState.Deleted));

        if (!entriesBeforeSave.Any())
        {
            Log.Warning("SaveChangesAsync called but ChangeTracker has no pending changes!");
            return 0;
        }

        var result = await base.SaveChangesAsync(ct);

        Log.Information("Successfully saved {Count} changes to database", result);

        return result;
    }
}
