using Microsoft.Extensions.Configuration;
using TC.Agro.Analytics.Application.Configuration;

namespace TC.Agro.Analytics.Application
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Configure AlertThresholds from appsettings.json
            services.Configure<AlertThresholdsOptions>(
                configuration.GetSection(AlertThresholdsOptions.SectionName));

            // Note: Query Handlers are registered in Infrastructure layer

            return services;
        }
    }
}
