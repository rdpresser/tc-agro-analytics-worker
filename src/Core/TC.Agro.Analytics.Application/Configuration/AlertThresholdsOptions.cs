namespace TC.Agro.Analytics.Application.Configuration
{
    /// <summary>
    /// Configuration options for alert thresholds.
    /// Bound from appsettings.json "AlertThresholds" section.
    /// </summary>
    public sealed class AlertThresholdsOptions
    {
        public const string SectionName = "AlertThresholds";

        /// <summary>
        /// Maximum temperature in Celsius before triggering high temperature alert.
        /// Default: 35°C
        /// </summary>
        public double MaxTemperature { get; set; } = 35;

        /// <summary>
        /// Minimum soil moisture percentage before triggering irrigation alert.
        /// Default: 20%
        /// </summary>
        public double MinSoilMoisture { get; set; } = 20;

        /// <summary>
        /// Minimum battery level percentage before triggering low battery warning.
        /// Default: 15%
        /// </summary>
        public double MinBatteryLevel { get; set; } = 15;
    }
}
