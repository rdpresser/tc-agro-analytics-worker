namespace TC.Agro.Analytics.Tests.Builders
{
    /// <summary>
    /// Builder pattern for creating SensorReadingAggregate test data
    /// </summary>
    public class SensorReadingAggregateBuilder
    {
        private string _sensorId = "SENSOR-001";
        private Guid _plotId = Guid.NewGuid();
        private DateTime _time = DateTime.UtcNow;
        private double? _temperature = 25.0;
        private double? _humidity = 60.0;
        private double? _soilMoisture = 40.0;
        private double? _rainfall = 5.0;
        private double? _batteryLevel = 85.0;

        public SensorReadingAggregateBuilder WithSensorId(string sensorId)
        {
            _sensorId = sensorId;
            return this;
        }

        public SensorReadingAggregateBuilder WithPlotId(Guid plotId)
        {
            _plotId = plotId;
            return this;
        }

        public SensorReadingAggregateBuilder WithTime(DateTime time)
        {
            _time = time;
            return this;
        }

        public SensorReadingAggregateBuilder WithTemperature(double? temperature)
        {
            _temperature = temperature;
            return this;
        }

        public SensorReadingAggregateBuilder WithHumidity(double? humidity)
        {
            _humidity = humidity;
            return this;
        }

        public SensorReadingAggregateBuilder WithSoilMoisture(double? soilMoisture)
        {
            _soilMoisture = soilMoisture;
            return this;
        }

        public SensorReadingAggregateBuilder WithRainfall(double? rainfall)
        {
            _rainfall = rainfall;
            return this;
        }

        public SensorReadingAggregateBuilder WithBatteryLevel(double? batteryLevel)
        {
            _batteryLevel = batteryLevel;
            return this;
        }

        /// <summary>
        /// Creates a high temperature scenario (above 35°C)
        /// </summary>
        public SensorReadingAggregateBuilder WithHighTemperature()
        {
            _temperature = 38.0;
            return this;
        }

        /// <summary>
        /// Creates a low soil moisture scenario (below 20%)
        /// </summary>
        public SensorReadingAggregateBuilder WithLowSoilMoisture()
        {
            _soilMoisture = 15.0;
            return this;
        }

        /// <summary>
        /// Creates a low battery scenario (below 15%)
        /// </summary>
        public SensorReadingAggregateBuilder WithLowBattery()
        {
            _batteryLevel = 10.0;
            return this;
        }

        /// <summary>
        /// Creates an aggregate without any metrics (should fail validation)
        /// </summary>
        public SensorReadingAggregateBuilder WithoutMetrics()
        {
            _temperature = null;
            _humidity = null;
            _soilMoisture = null;
            _rainfall = null;
            return this;
        }

        public Result<SensorReadingAggregate> Build()
        {
            return SensorReadingAggregate.Create(
                sensorId: _sensorId,
                plotId: _plotId,
                time: _time,
                temperature: _temperature,
                humidity: _humidity,
                soilMoisture: _soilMoisture,
                rainfall: _rainfall,
                batteryLevel: _batteryLevel
            );
        }
    }
}
