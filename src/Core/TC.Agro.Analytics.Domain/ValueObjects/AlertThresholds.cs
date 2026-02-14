namespace TC.Agro.Analytics.Domain.ValueObjects
{
    public sealed record AlertThresholds
    {
        public double MaxTemperature { get; init; }
        public double MinSoilMoisture { get; init; }
        public double MinBatteryLevel { get; init; }

        public AlertThresholds(
            double maxTemperature = 35,
            double minSoilMoisture = 20,
            double minBatteryLevel = 15)
        {
            MaxTemperature = maxTemperature;
            MinSoilMoisture = minSoilMoisture;
            MinBatteryLevel = minBatteryLevel;
        }

        public static AlertThresholds Default => new();
    }
}
