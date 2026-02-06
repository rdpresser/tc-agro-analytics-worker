using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Domain.Abstractions.Ports;
using TC.Agro.Analytics.Infrastructure.Messaging;
using TC.Agro.Analytics.Infrastructure.Projections;
using TC.Agro.Analytics.Infrastructure.Repositores;
using TC.Agro.Analytics.Infrastructure.Stores;
using TC.Agro.SharedKernel.Application.Ports;

namespace TC.Agro.Analytics.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext with explicit Npgsql provider configuration
            // Wolverine will integrate automatically via UseEntityFrameworkCoreTransactions()
            services.AddDbContext<ApplicationDbContext>((sp, opts) =>
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
            }, 
            contextLifetime: ServiceLifetime.Scoped, 
            optionsLifetime: ServiceLifetime.Scoped);

            SharedKernel.Infrastructure.DependencyInjection.AddAgroInfrastructure(services, configuration);

            // Register Repositories (Write side - CQRS)
            services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();

            // Register Read Stores (Read side - CQRS)
            services.AddScoped<IAlertReadStore, AlertReadStore>();

            // Register Transactional Outbox (Wolverine + EF Core)
            services.AddScoped<ITransactionalOutbox, WolverineEfCoreOutbox>();

            // Register Projection Handlers (Wolverine will auto-discover them)
            services.AddScoped<AlertProjectionHandler>();

            return services;
        }
    }
}
