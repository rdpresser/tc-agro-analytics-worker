using TC.Agro.Analytics.Tests.Builders;

namespace TC.Agro.Analytics.Tests.Domain.Aggregates
{
    /// <summary>
    /// Unit tests for SensorReadingAggregate - Domain Layer
    /// Tests business rules, validations, and domain events
    /// </summary>
    public class SensorReadingAggregateTests
    {
        #region Create Tests

        [Fact]
        public void Create_WithValidData_ShouldSucceed()
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder().Build();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.SensorId.ShouldBe("SENSOR-001");
            result.Value.Temperature.ShouldBe(25.0);
        }

        [Fact]
        public void Create_WithValidData_ShouldRaiseSensorReadingCreatedDomainEvent()
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder().Build();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var aggregate = result.Value;

            aggregate.UncommittedEvents.ShouldNotBeEmpty();
            aggregate.UncommittedEvents.Count.ShouldBe(1);
            aggregate.UncommittedEvents.ToList()[0].ShouldBeOfType<SensorReadingAggregate.SensorReadingCreatedDomainEvent>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidSensorId_ShouldFail(string? invalidSensorId)
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithSensorId(invalidSensorId!)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("SensorId.Required"));
        }

        [Fact]
        public void Create_WithSensorIdTooLong_ShouldFail()
        {
            // Arrange
            var longSensorId = new string('A', 101); // > 100 characters

            // Act
            var result = new SensorReadingAggregateBuilder()
                .WithSensorId(longSensorId)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("SensorId.TooLong"));
        }

        [Fact]
        public void Create_WithEmptyPlotId_ShouldFail()
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithPlotId(Guid.Empty)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("PlotId.Required"));
        }

        [Fact]
        public void Create_WithFutureTime_ShouldFail()
        {
            // Arrange
            var futureTime = DateTime.UtcNow.AddHours(1);

            // Act
            var result = new SensorReadingAggregateBuilder()
                .WithTime(futureTime)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("Time.FutureNotAllowed"));
        }

        [Fact]
        public void Create_WithoutAnyMetrics_ShouldFail()
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithoutMetrics()
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("Metrics.Required"));
        }

        [Theory]
        [InlineData(-51)] // Below minimum
        [InlineData(71)]  // Above maximum
        public void Create_WithTemperatureOutOfRange_ShouldFail(double invalidTemperature)
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithTemperature(invalidTemperature)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("Temperature.OutOfRange"));
        }

        [Theory]
        [InlineData(-1)]  // Below minimum
        [InlineData(101)] // Above maximum
        public void Create_WithHumidityOutOfRange_ShouldFail(double invalidHumidity)
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithHumidity(invalidHumidity)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("Humidity.OutOfRange"));
        }

        [Theory]
        [InlineData(-1)]  // Below minimum
        [InlineData(101)] // Above maximum
        public void Create_WithSoilMoistureOutOfRange_ShouldFail(double invalidSoilMoisture)
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithSoilMoisture(invalidSoilMoisture)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("SoilMoisture.OutOfRange"));
        }

        [Fact]
        public void Create_WithNegativeRainfall_ShouldFail()
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithRainfall(-1)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("Rainfall.OutOfRange"));
        }

        [Theory]
        [InlineData(-1)]  // Below minimum
        [InlineData(101)] // Above maximum
        public void Create_WithBatteryLevelOutOfRange_ShouldFail(double invalidBatteryLevel)
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithBatteryLevel(invalidBatteryLevel)
                .Build();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(e => e.Identifier.Contains("BatteryLevel.OutOfRange"));
        }

        #endregion

        #region EvaluateAlerts Tests

        [Fact]
        public void EvaluateAlerts_WithHighTemperature_ShouldRaiseHighTemperatureEvent()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithHighTemperature() // 38°C
                .Build().Value;

            var thresholds = new AlertThresholds(maxTemperature: 35);

            // Act
            aggregate.EvaluateAlerts(thresholds);

            // Assert
            var alertEvents = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.HighTemperatureDetectedDomainEvent>()
                .ToList();

            alertEvents.ShouldNotBeEmpty();
            alertEvents.Count.ShouldBe(1);
            alertEvents[0].Temperature.ShouldBe(38.0);
        }

        [Fact]
        public void EvaluateAlerts_WithNormalTemperature_ShouldNotRaiseTemperatureEvent()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithTemperature(30.0) // Normal temperature
                .Build().Value;

            var thresholds = new AlertThresholds(maxTemperature: 35);

            // Act
            aggregate.EvaluateAlerts(thresholds);

            // Assert
            var alertEvents = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.HighTemperatureDetectedDomainEvent>()
                .ToList();

            alertEvents.ShouldBeEmpty();
        }

        [Fact]
        public void EvaluateAlerts_WithLowSoilMoisture_ShouldRaiseLowSoilMoistureEvent()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithLowSoilMoisture() // 15%
                .Build().Value;

            var thresholds = new AlertThresholds(minSoilMoisture: 20);

            // Act
            aggregate.EvaluateAlerts(thresholds);

            // Assert
            var alertEvents = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.LowSoilMoistureDetectedDomainEvent>()
                .ToList();

            alertEvents.ShouldNotBeEmpty();
            alertEvents.Count.ShouldBe(1);
            alertEvents[0].SoilMoisture.ShouldBe(15.0);
        }

        [Fact]
        public void EvaluateAlerts_WithNormalSoilMoisture_ShouldNotRaiseSoilMoistureEvent()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithSoilMoisture(40.0) // Normal
                .Build().Value;

            var thresholds = new AlertThresholds(minSoilMoisture: 20);

            // Act
            aggregate.EvaluateAlerts(thresholds);

            // Assert
            var alertEvents = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.LowSoilMoistureDetectedDomainEvent>()
                .ToList();

            alertEvents.ShouldBeEmpty();
        }

        [Fact]
        public void EvaluateAlerts_WithLowBattery_ShouldRaiseBatteryLowWarningEvent()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithLowBattery() // 10%
                .Build().Value;

            var thresholds = new AlertThresholds(minBatteryLevel: 15);

            // Act
            aggregate.EvaluateAlerts(thresholds);

            // Assert
            var alertEvents = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.BatteryLowWarningDomainEvent>()
                .ToList();

            alertEvents.ShouldNotBeEmpty();
            alertEvents.Count.ShouldBe(1);
            alertEvents[0].BatteryLevel.ShouldBe(10.0);
            alertEvents[0].Threshold.ShouldBe(15.0);
        }

        [Fact]
        public void EvaluateAlerts_WithMultipleAlertsTriggered_ShouldRaiseAllEvents()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithHighTemperature()    // 38°C
                .WithLowSoilMoisture()    // 15%
                .WithLowBattery()         // 10%
                .Build().Value;

            var thresholds = AlertThresholds.Default;

            // Act
            aggregate.EvaluateAlerts(thresholds);

            // Assert
            var allAlertEvents = aggregate.UncommittedEvents
                .Where(e => e is SensorReadingAggregate.HighTemperatureDetectedDomainEvent
                         or SensorReadingAggregate.LowSoilMoistureDetectedDomainEvent
                         or SensorReadingAggregate.BatteryLowWarningDomainEvent)
                .ToList();

            allAlertEvents.Count.ShouldBe(3);
        }

        [Fact]
        public void EvaluateAlerts_WithNullTemperature_ShouldNotRaiseTemperatureEvent()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithTemperature(null)
                .Build().Value;

            var thresholds = AlertThresholds.Default;

            // Act
            aggregate.EvaluateAlerts(thresholds);

            // Assert
            var alertEvents = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.HighTemperatureDetectedDomainEvent>()
                .ToList();

            alertEvents.ShouldBeEmpty();
        }

        [Fact]
        public void EvaluateAlerts_WithCustomThresholds_ShouldRespectThresholds()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithTemperature(32.0) // Below default (35) but above custom (30)
                .Build().Value;

            var customThresholds = new AlertThresholds(maxTemperature: 30);

            // Act
            aggregate.EvaluateAlerts(customThresholds);

            // Assert
            var alertEvents = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.HighTemperatureDetectedDomainEvent>()
                .ToList();

            alertEvents.ShouldNotBeEmpty();
        }

        #endregion

        #region Domain Event Tests

        [Fact]
        public void SensorReadingCreatedDomainEvent_ShouldHaveCorrectData()
        {
            // Arrange & Act
            var result = new SensorReadingAggregateBuilder()
                .WithSensorId("SENSOR-123")
                .WithTemperature(28.5)
                .Build();

            // Assert
            var createdEvent = result.Value.UncommittedEvents
                .OfType<SensorReadingAggregate.SensorReadingCreatedDomainEvent>()
                .First();

            createdEvent.SensorId.ShouldBe("SENSOR-123");
            createdEvent.Temperature.ShouldBe(28.5);
            createdEvent.AggregateId.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public void HighTemperatureDetectedDomainEvent_ShouldHaveCorrectData()
        {
            // Arrange
            var aggregate = new SensorReadingAggregateBuilder()
                .WithSensorId("SENSOR-HOT")
                .WithHighTemperature()
                .Build().Value;

            // Act
            aggregate.EvaluateAlerts(AlertThresholds.Default);

            // Assert
            var alertEvent = aggregate.UncommittedEvents
                .OfType<SensorReadingAggregate.HighTemperatureDetectedDomainEvent>()
                .First();

            alertEvent.SensorId.ShouldBe("SENSOR-HOT");
            alertEvent.Temperature.ShouldBe(38.0);
        }

        #endregion
    }
}
