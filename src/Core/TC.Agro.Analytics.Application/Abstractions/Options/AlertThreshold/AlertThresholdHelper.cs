using Microsoft.Extensions.Configuration;

namespace TC.Agro.Analytics.Application.Abstractions.Options.AlertThreshold
{
    public sealed class AlertThresholdHelper
    {
        public AlertThresholdOptions AlertThresholdSettings { get; }

        public AlertThresholdHelper(IConfiguration configuration)
        {
            AlertThresholdSettings = configuration.GetSection(AlertThresholdOptions.SectionName).Get<AlertThresholdOptions>()
                                     ?? new AlertThresholdOptions();
        }

        public static AlertThresholdOptions Build(IConfiguration configuration) =>
            new AlertThresholdHelper(configuration).AlertThresholdSettings;
    }
}
