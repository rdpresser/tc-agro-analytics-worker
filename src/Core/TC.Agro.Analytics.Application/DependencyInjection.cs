namespace TC.Agro.Analytics.Application;

using TC.Agro.Analytics.Application.UseCases.ProcessSensorAlerts;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Configure AlertThresholds from appsettings.json (Alerts:Thresholds section)
            services.Configure<AlertThresholdsOptions>(
                configuration.GetSection(AlertThresholdsOptions.ConfigurationSection));

            // Register command handler for processing sensor alerts
            services.AddScoped<ProcessSensorAlertsCommandHandler>();

            // Query Handlers are registered automatically by SharedKernel
            // (via reflection - all classes implementing BaseQueryHandler)
            return services;
        }
    }
