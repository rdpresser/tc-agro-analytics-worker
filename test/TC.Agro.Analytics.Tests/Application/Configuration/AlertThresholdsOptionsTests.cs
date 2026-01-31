namespace TC.Agro.Analytics.Tests.Application.Configuration
{
    /// <summary>
    /// Unit tests for AlertThresholdsOptions configuration class
    /// </summary>
    public class AlertThresholdsOptionsTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var options = new AlertThresholdsOptions();

            // Assert
            options.MaxTemperature.ShouldBe(35.0);
            options.MinSoilMoisture.ShouldBe(20.0);
            options.MinBatteryLevel.ShouldBe(15.0);
        }

        [Fact]
        public void SectionName_ShouldBeAlertThresholds()
        {
            // Arrange & Act & Assert
            AlertThresholdsOptions.SectionName.ShouldBe("AlertThresholds");
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var options = new AlertThresholdsOptions();

            // Act
            options.MaxTemperature = 40.0;
            options.MinSoilMoisture = 25.0;
            options.MinBatteryLevel = 20.0;

            // Assert
            options.MaxTemperature.ShouldBe(40.0);
            options.MinSoilMoisture.ShouldBe(25.0);
            options.MinBatteryLevel.ShouldBe(20.0);
        }

        [Fact]
        public void MapTo_AlertThresholds_ShouldCreateValidValueObject()
        {
            // Arrange
            var options = new AlertThresholdsOptions
            {
                MaxTemperature = 40.0,
                MinSoilMoisture = 25.0,
                MinBatteryLevel = 20.0
            };

            // Act
            var thresholds = new AlertThresholds(
                options.MaxTemperature,
                options.MinSoilMoisture,
                options.MinBatteryLevel
            );

            // Assert
            thresholds.MaxTemperature.ShouldBe(40.0);
            thresholds.MinSoilMoisture.ShouldBe(25.0);
            thresholds.MinBatteryLevel.ShouldBe(20.0);
        }
    }
}
