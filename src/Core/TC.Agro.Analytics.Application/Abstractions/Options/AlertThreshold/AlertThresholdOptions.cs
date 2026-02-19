namespace TC.Agro.Analytics.Application.Abstractions.Options.AlertThreshold
{
    /// <summary>
    /// Configuration options for alert thresholds.
    /// Configured via appsettings.json under "Alerts:Thresholds" section.
    /// Example:
    /// {
    ///   "Alerts": {
    ///     "Thresholds": {
    ///       "MaxTemperature": 35.0,
    ///       "MinSoilMoisture": 20.0,
    ///       "MinBatteryLevel": 15.0
    ///     }
    ///   }
    /// }
    /// </summary>
    public sealed class AlertThresholdOptions
    {
        public const string SectionName = "Alerts:Thresholds";
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
