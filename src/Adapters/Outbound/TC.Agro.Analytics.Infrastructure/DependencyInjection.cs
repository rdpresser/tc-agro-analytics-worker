using TC.Agro.Analytics.Infrastructure.Projections;
using TC.Agro.Analytics.Infrastructure.Queries;

namespace TC.Agro.Analytics.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);

            SharedKernel.Infrastructure.DependencyInjection.AddAgroInfrastructure(services, configuration);

            // Register Projection Handlers (Wolverine will auto-discover them)
            services.AddScoped<AlertProjectionHandler>();

            // Register Query Handlers (CQRS Query Side)
            services.AddScoped<GetPendingAlertsQueryHandler>();
            services.AddScoped<GetAlertHistoryQueryHandler>();
            services.AddScoped<GetPlotStatusQueryHandler>();

            return services;
        }
    }
}
