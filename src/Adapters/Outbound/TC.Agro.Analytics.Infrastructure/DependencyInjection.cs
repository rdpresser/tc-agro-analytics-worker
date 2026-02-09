namespace TC.Agro.Analytics.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            // -------------------------------
            // EF Core with Wolverine Integration
            // IMPORTANT: Use AddDbContextWithWolverineIntegration instead of AddDbContext
            // This enables the transactional outbox pattern with Wolverine
            // -------------------------------
            services.AddDbContextWithWolverineIntegration<ApplicationDbContext>((sp, opts) =>
            {
                var dbFactory = sp.GetRequiredService<DbConnectionFactory>();

                opts.UseNpgsql(dbFactory.ConnectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, DefaultSchemas.Default);
                })
                .UseSnakeCaseNamingConvention();

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    opts.EnableSensitiveDataLogging(true);
                    opts.EnableDetailedErrors();
                }
            });

            // Register ApplicationDbContext as IApplicationDbContext (required for ApplyMigrations)
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            SharedKernel.Infrastructure.DependencyInjection.AddAgroInfrastructure(services, configuration);

            // Register Repositories (Write side - CQRS)
            services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();

            // Register Read Stores (Read side - CQRS)
            services.AddScoped<IAlertReadStore, AlertReadStore>();

            // Register Transactional Outbox (Wolverine + EF Core)
            services.AddScoped<ITransactionalOutbox, AnalyticsOutbox>();

            return services;
        }
    }
