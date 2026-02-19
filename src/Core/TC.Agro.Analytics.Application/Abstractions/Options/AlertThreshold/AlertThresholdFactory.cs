using Microsoft.Extensions.Options;

namespace TC.Agro.Analytics.Application.Abstractions.Options.AlertThreshold
{
    public sealed class AlertThresholdFactory
    {
        private readonly AlertThresholdOptions _options;

        public AlertThresholdFactory(IOptions<AlertThresholdOptions> options)
        {
            _options = options.Value;
        }

        public double MaxTemperature => _options.MaxTemperature;
        public double MinSoilMoisture => _options.MinSoilMoisture;
        public double MinBatteryLevel => _options.MinBatteryLevel;
    }
}
