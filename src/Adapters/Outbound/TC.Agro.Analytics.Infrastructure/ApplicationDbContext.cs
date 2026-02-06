using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Domain.Entities;
using TC.Agro.SharedKernel.Infrastructure.Database;
using TC.Agro.SharedKernel.Infrastructure.Database.EfCore;

namespace TC.Agro.Analytics.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Implementation of IApplicationDbContext.DbContext
        /// Returns this instance cast to DbContext for advanced scenarios.
        /// </summary>
        DbContext IApplicationDbContext.DbContext => this;

        /// <summary>
        /// Alerts read model (projected from domain events)
        /// </summary>
        public DbSet<Alert> Alerts { get; set; } = default!;

        /// <summary>
        /// Sensor readings aggregate root (write model)
        /// </summary>
        public DbSet<SensorReadingAggregate> SensorReadings { get; set; } = default!;

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
                            new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
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
}
