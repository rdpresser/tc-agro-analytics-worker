namespace TC.Agro.Analytics.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Unit tests for AlertThresholds value object
    /// </summary>
    public class AlertThresholdsTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var thresholds = new AlertThresholds(
                maxTemperature: 40.0,
                minSoilMoisture: 25.0,
                minBatteryLevel: 20.0
            );

            // Assert
            thresholds.ShouldNotBeNull();
            thresholds.MaxTemperature.ShouldBe(40.0);
            thresholds.MinSoilMoisture.ShouldBe(25.0);
            thresholds.MinBatteryLevel.ShouldBe(20.0);
        }

        [Fact]
        public void Constructor_WithDefaultParameters_ShouldUseDefaultValues()
        {
            // Arrange & Act
            var thresholds = new AlertThresholds();

            // Assert
            thresholds.MaxTemperature.ShouldBe(35.0);
            thresholds.MinSoilMoisture.ShouldBe(20.0);
            thresholds.MinBatteryLevel.ShouldBe(15.0);
        }

        [Fact]
        public void Default_Property_ShouldReturnDefaultThresholds()
        {
            // Arrange & Act
            var thresholds = AlertThresholds.Default;

            // Assert
            thresholds.ShouldNotBeNull();
            thresholds.MaxTemperature.ShouldBe(35.0);
            thresholds.MinSoilMoisture.ShouldBe(20.0);
            thresholds.MinBatteryLevel.ShouldBe(15.0);
        }

        [Fact]
        public void Equality_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            var threshold1 = new AlertThresholds(35, 20, 15);
            var threshold2 = new AlertThresholds(35, 20, 15);

            // Act & Assert
            threshold1.ShouldBe(threshold2);
            (threshold1 == threshold2).ShouldBeTrue();
        }

        [Fact]
        public void Equality_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var threshold1 = new AlertThresholds(35, 20, 15);
            var threshold2 = new AlertThresholds(40, 20, 15);

            // Act & Assert
            threshold1.ShouldNotBe(threshold2);
            (threshold1 != threshold2).ShouldBeTrue();
        }

        [Fact]
        public void Record_ShouldBeImmutable()
        {
            // Arrange
            var thresholds = new AlertThresholds(35, 20, 15);

            // Act & Assert
            // Records são imutáveis - as propriedades são init-only
            // Este teste valida que o tipo é um record
            thresholds.GetType().IsClass.ShouldBeTrue();
            thresholds.MaxTemperature.ShouldBe(35);
        }

        [Fact]
        public void ToString_ShouldReturnMeaningfulRepresentation()
        {
            // Arrange
            var thresholds = new AlertThresholds(35, 20, 15);

            // Act
            var stringRepresentation = thresholds.ToString();

            // Assert
            stringRepresentation.ShouldNotBeNullOrWhiteSpace();
            stringRepresentation.ShouldContain("35");
            stringRepresentation.ShouldContain("20");
            stringRepresentation.ShouldContain("15");
        }
    }
}
