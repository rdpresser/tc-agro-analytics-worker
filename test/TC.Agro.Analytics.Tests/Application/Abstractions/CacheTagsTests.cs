using TC.Agro.Analytics.Application.Abstractions;

namespace TC.Agro.Analytics.Tests.Application.Abstractions;

/// <summary>
/// Unit tests for CacheTags.
/// </summary>
public class CacheTagsTests
{
    [Fact]
    public void CacheTags_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        CacheTags.Alerts.ShouldBe("alerts");
        CacheTags.PendingAlerts.ShouldBe("alerts:pending");
        CacheTags.SensorReadings.ShouldBe("sensor-readings");
        CacheTags.PlotStatus.ShouldBe("plot-status");
    }

    [Fact]
    public void AlertById_WithValidGuid_ShouldReturnCorrectTag()
    {
        // Arrange
        var alertId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683");

        // Act
        var tag = CacheTags.AlertById(alertId);

        // Assert
        tag.ShouldBe("alert:ae57f8d7-d491-4899-bb39-30124093e683");
    }

    [Fact]
    public void AlertById_WithDifferentGuids_ShouldReturnDifferentTags()
    {
        // Arrange
        var alertId1 = Guid.NewGuid();
        var alertId2 = Guid.NewGuid();

        // Act
        var tag1 = CacheTags.AlertById(alertId1);
        var tag2 = CacheTags.AlertById(alertId2);

        // Assert
        tag1.ShouldNotBe(tag2);
        tag1.ShouldStartWith("alert:");
        tag2.ShouldStartWith("alert:");
    }

    [Fact]
    public void AlertsByPlot_WithValidGuid_ShouldReturnCorrectTag()
    {
        // Arrange
        var plotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683");

        // Act
        var tag = CacheTags.AlertsByPlot(plotId);

        // Assert
        tag.ShouldBe("alerts:plot:ae57f8d7-d491-4899-bb39-30124093e683");
    }

    [Fact]
    public void AlertsBySensor_WithValidSensorId_ShouldReturnCorrectTag()
    {
        // Arrange
        var sensorId = "SENSOR-001";

        // Act
        var tag = CacheTags.AlertsBySensor(sensorId);

        // Assert
        tag.ShouldBe("alerts:sensor:SENSOR-001");
    }

    [Theory]
    [InlineData("SENSOR-001")]
    [InlineData("SENSOR-HOT")]
    [InlineData("SENSOR-XYZ-123")]
    public void AlertsBySensor_WithDifferentSensorIds_ShouldReturnCorrectTags(string sensorId)
    {
        // Act
        var tag = CacheTags.AlertsBySensor(sensorId);

        // Assert
        tag.ShouldBe($"alerts:sensor:{sensorId}");
    }

    [Fact]
    public void PlotStatusById_WithValidGuid_ShouldReturnCorrectTag()
    {
        // Arrange
        var plotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683");

        // Act
        var tag = CacheTags.PlotStatusById(plotId);

        // Assert
        tag.ShouldBe("plot-status:ae57f8d7-d491-4899-bb39-30124093e683");
    }

    [Fact]
    public void SensorReadingById_WithValidGuid_ShouldReturnCorrectTag()
    {
        // Arrange
        var readingId = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        // Act
        var tag = CacheTags.SensorReadingById(readingId);

        // Assert
        tag.ShouldBe("sensor-reading:12345678-1234-1234-1234-123456789abc");
    }

    [Fact]
    public void CacheTags_AllMethods_ShouldReturnNonEmptyStrings()
    {
        // Arrange
        var testGuid = Guid.NewGuid();
        var testSensorId = "TEST-SENSOR";

        // Act & Assert
        CacheTags.AlertById(testGuid).ShouldNotBeNullOrWhiteSpace();
        CacheTags.AlertsByPlot(testGuid).ShouldNotBeNullOrWhiteSpace();
        CacheTags.AlertsBySensor(testSensorId).ShouldNotBeNullOrWhiteSpace();
        CacheTags.PlotStatusById(testGuid).ShouldNotBeNullOrWhiteSpace();
        CacheTags.SensorReadingById(testGuid).ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CacheTags_SameInputSameOutput_ShouldBeConsistent()
    {
        // Arrange
        var plotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683");

        // Act
        var tag1 = CacheTags.AlertsByPlot(plotId);
        var tag2 = CacheTags.AlertsByPlot(plotId);

        // Assert
        tag1.ShouldBe(tag2);
    }
}
