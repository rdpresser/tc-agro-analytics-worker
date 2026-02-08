namespace TC.Agro.Analytics.Tests.Domain.ValueObjects;

/// <summary>
/// Unit tests for AlertType Value Object
/// </summary>
public class AlertTypeTests
{
    #region Factory Method Tests

    [Theory]
    [InlineData("HighTemperature")]
    [InlineData("LowSoilMoisture")]
    [InlineData("LowBattery")]
    public void Create_WithValidType_ShouldSucceed(string typeValue)
    {
        // Arrange & Act
        var result = AlertType.Create(typeValue);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(typeValue);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("HIGHTEMPERATURE")]
    [InlineData("hightemperature")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithInvalidType_ShouldFail(string? invalidType)
    {
        // Arrange & Act
        var result = AlertType.Create(invalidType!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(e => e.Identifier == "AlertType.Invalid");
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void HighTemperature_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var type = AlertType.HighTemperature;

        // Assert
        type.Value.ShouldBe("HighTemperature");
        type.IsHighTemperature.ShouldBeTrue();
        type.IsLowSoilMoisture.ShouldBeFalse();
        type.IsLowBattery.ShouldBeFalse();
    }

    [Fact]
    public void LowSoilMoisture_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var type = AlertType.LowSoilMoisture;

        // Assert
        type.Value.ShouldBe("LowSoilMoisture");
        type.IsHighTemperature.ShouldBeFalse();
        type.IsLowSoilMoisture.ShouldBeTrue();
        type.IsLowBattery.ShouldBeFalse();
    }

    [Fact]
    public void LowBattery_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var type = AlertType.LowBattery;

        // Assert
        type.Value.ShouldBe("LowBattery");
        type.IsHighTemperature.ShouldBeFalse();
        type.IsLowSoilMoisture.ShouldBeFalse();
        type.IsLowBattery.ShouldBeTrue();
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_ShouldReturnAllTypes()
    {
        // Arrange & Act
        var allTypes = AlertType.GetAll().ToList();

        // Assert
        allTypes.ShouldNotBeEmpty();
        allTypes.Count.ShouldBe(3);
        allTypes.ShouldContain(AlertType.HighTemperature);
        allTypes.ShouldContain(AlertType.LowSoilMoisture);
        allTypes.ShouldContain(AlertType.LowBattery);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToString_ShouldWork()
    {
        // Arrange
        AlertType type = AlertType.HighTemperature;

        // Act
        string stringValue = type;

        // Assert
        stringValue.ShouldBe("HighTemperature");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equality_SameType_ShouldBeEqual()
    {
        // Arrange
        var type1 = AlertType.HighTemperature;
        var type2 = AlertType.HighTemperature;

        // Act & Assert
        (type1 == type2).ShouldBeTrue();
        type1.Equals(type2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentType_ShouldNotBeEqual()
    {
        // Arrange
        var type1 = AlertType.HighTemperature;
        var type2 = AlertType.LowBattery;

        // Act & Assert
        (type1 == type2).ShouldBeFalse();
        type1.Equals(type2).ShouldBeFalse();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var type = AlertType.LowSoilMoisture;

        // Act
        var result = type.ToString();

        // Assert
        result.ShouldBe("LowSoilMoisture");
    }

    #endregion
}
