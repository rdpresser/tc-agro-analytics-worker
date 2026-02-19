namespace TC.Agro.Analytics.Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Configure AlertThresholds from appsettings.json (Alerts:Thresholds section)
            ////services.Configure<AlertThresholdsOptions>(configuration.GetSection(AlertThresholdsOptions.ConfigurationSection));

            // Query Handlers are registered automatically by SharedKernel
            // (via reflection - all classes implementing BaseQueryHandler)
            return services;
        }
    }
