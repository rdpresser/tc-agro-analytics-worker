namespace TC.Agro.Analytics.Tests.Domain.ValueObjects;

/// <summary>
/// Unit tests for AlertSeverity Value Object
/// </summary>
public class AlertSeverityTests
{
    #region Factory Method Tests

    [Theory]
    [InlineData("Low", 1)]
    [InlineData("Medium", 2)]
    [InlineData("High", 3)]
    [InlineData("Critical", 4)]
    public void Create_WithValidSeverity_ShouldSucceed(string severityValue, int expectedLevel)
    {
        // Arrange & Act
        var result = AlertSeverity.Create(severityValue);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(severityValue);
        result.Value.Level.ShouldBe(expectedLevel);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("LOW")]
    [InlineData("low")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithInvalidSeverity_ShouldFail(string? invalidSeverity)
    {
        // Arrange & Act
        var result = AlertSeverity.Create(invalidSeverity!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(e => e.Identifier == "AlertSeverity.Invalid");
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void Low_ShouldHaveCorrectValueAndLevel()
    {
        // Arrange & Act
        var severity = AlertSeverity.Low;

        // Assert
        severity.Value.ShouldBe("Low");
        severity.Level.ShouldBe(1);
        severity.IsLow.ShouldBeTrue();
        severity.IsMedium.ShouldBeFalse();
        severity.IsHigh.ShouldBeFalse();
        severity.IsCritical.ShouldBeFalse();
    }

    [Fact]
    public void Medium_ShouldHaveCorrectValueAndLevel()
    {
        // Arrange & Act
        var severity = AlertSeverity.Medium;

        // Assert
        severity.Value.ShouldBe("Medium");
        severity.Level.ShouldBe(2);
        severity.IsLow.ShouldBeFalse();
        severity.IsMedium.ShouldBeTrue();
        severity.IsHigh.ShouldBeFalse();
        severity.IsCritical.ShouldBeFalse();
    }

    [Fact]
    public void High_ShouldHaveCorrectValueAndLevel()
    {
        // Arrange & Act
        var severity = AlertSeverity.High;

        // Assert
        severity.Value.ShouldBe("High");
        severity.Level.ShouldBe(3);
        severity.IsLow.ShouldBeFalse();
        severity.IsMedium.ShouldBeFalse();
        severity.IsHigh.ShouldBeTrue();
        severity.IsCritical.ShouldBeFalse();
    }

    [Fact]
    public void Critical_ShouldHaveCorrectValueAndLevel()
    {
        // Arrange & Act
        var severity = AlertSeverity.Critical;

        // Assert
        severity.Value.ShouldBe("Critical");
        severity.Level.ShouldBe(4);
        severity.IsLow.ShouldBeFalse();
        severity.IsMedium.ShouldBeFalse();
        severity.IsHigh.ShouldBeFalse();
        severity.IsCritical.ShouldBeTrue();
    }

    #endregion

    #region Comparison Operators Tests

    [Fact]
    public void GreaterThanOperator_CriticalVsHigh_ShouldBeTrue()
    {
        // Arrange
        var critical = AlertSeverity.Critical;
        var high = AlertSeverity.High;

        // Act & Assert
        (critical > high).ShouldBeTrue();
        (high > critical).ShouldBeFalse();
    }

    [Fact]
    public void LessThanOperator_LowVsMedium_ShouldBeTrue()
    {
        // Arrange
        var low = AlertSeverity.Low;
        var medium = AlertSeverity.Medium;

        // Act & Assert
        (low < medium).ShouldBeTrue();
        (medium < low).ShouldBeFalse();
    }

    [Fact]
    public void GreaterThanOrEqualOperator_SameSeverity_ShouldBeTrue()
    {
        // Arrange
        var high1 = AlertSeverity.High;
        var high2 = AlertSeverity.High;

        // Act & Assert
        (high1 >= high2).ShouldBeTrue();
    }

    [Fact]
    public void LessThanOrEqualOperator_SameSeverity_ShouldBeTrue()
    {
        // Arrange
        var medium1 = AlertSeverity.Medium;
        var medium2 = AlertSeverity.Medium;

        // Act & Assert
        (medium1 <= medium2).ShouldBeTrue();
    }

    [Fact]
    public void ComparisonOperators_AllSeverities_ShouldWorkCorrectly()
    {
        // Arrange
        var low = AlertSeverity.Low;
        var medium = AlertSeverity.Medium;
        var high = AlertSeverity.High;
        var critical = AlertSeverity.Critical;

        // Act & Assert
        (low < medium).ShouldBeTrue();
        (medium < high).ShouldBeTrue();
        (high < critical).ShouldBeTrue();
        (critical > high).ShouldBeTrue();
        (high > medium).ShouldBeTrue();
        (medium > low).ShouldBeTrue();
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_ShouldReturnAllSeverities()
    {
        // Arrange & Act
        var allSeverities = AlertSeverity.GetAll().ToList();

        // Assert
        allSeverities.ShouldNotBeEmpty();
        allSeverities.Count.ShouldBe(4);
        allSeverities.ShouldContain(AlertSeverity.Low);
        allSeverities.ShouldContain(AlertSeverity.Medium);
        allSeverities.ShouldContain(AlertSeverity.High);
        allSeverities.ShouldContain(AlertSeverity.Critical);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToString_ShouldWork()
    {
        // Arrange
        AlertSeverity severity = AlertSeverity.High;

        // Act
        string stringValue = severity;

        // Assert
        stringValue.ShouldBe("High");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equality_SameSeverity_ShouldBeEqual()
    {
        // Arrange
        var severity1 = AlertSeverity.High;
        var severity2 = AlertSeverity.High;

        // Act & Assert
        (severity1 == severity2).ShouldBeTrue();
        severity1.Equals(severity2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentSeverity_ShouldNotBeEqual()
    {
        // Arrange
        var severity1 = AlertSeverity.Low;
        var severity2 = AlertSeverity.Critical;

        // Act & Assert
        (severity1 == severity2).ShouldBeFalse();
        severity1.Equals(severity2).ShouldBeFalse();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var severity = AlertSeverity.Critical;

        // Act
        var result = severity.ToString();

        // Assert
        result.ShouldBe("Critical");
    }

    #endregion
}
