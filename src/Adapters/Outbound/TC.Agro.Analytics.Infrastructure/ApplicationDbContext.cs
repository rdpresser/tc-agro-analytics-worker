using TC.Agro.Analytics.Domain.Entities;

namespace TC.Agro.Analytics.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        public ApplicationDbContext(DbContextOptions options, DbConnectionFactory dbConnectionFactory)
            : base(options)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Alerts read model (projected from domain events)
        /// </summary>
        public DbSet<Alert> Alerts { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                base.OnConfiguring(optionsBuilder);

                // Configure the DbContext to use PostgreSQL with the connection string from the provider
                optionsBuilder.UseNpgsql(_dbConnectionFactory.ConnectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention();

                // Enable lazy loading proxies
                ////optionsBuilder.UseLazyLoadingProxies();

                // Use Serilog for EF Core logging
                optionsBuilder.LogTo(Log.Logger.Information, LogLevel.Information);

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    optionsBuilder.EnableSensitiveDataLogging(true);
                    optionsBuilder.EnableDetailedErrors();
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.HasDefaultSchema(Schemas.Default);
        }
    }
}
