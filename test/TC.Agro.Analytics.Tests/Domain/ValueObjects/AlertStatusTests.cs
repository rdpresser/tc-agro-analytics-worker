namespace TC.Agro.Analytics.Tests.Domain.ValueObjects;

/// <summary>
/// Unit tests for AlertStatus Value Object
/// </summary>
public class AlertStatusTests
{
    #region Factory Method Tests

    [Fact]
    public void Create_WithValidPendingStatus_ShouldSucceed()
    {
        // Arrange & Act
        var result = AlertStatus.Create("Pending");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(AlertStatus.Pending);
        result.Value.IsPending.ShouldBeTrue();
        result.Value.IsAcknowledged.ShouldBeFalse();
        result.Value.IsResolved.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithValidAcknowledgedStatus_ShouldSucceed()
    {
        // Arrange & Act
        var result = AlertStatus.Create("Acknowledged");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(AlertStatus.Acknowledged);
        result.Value.IsPending.ShouldBeFalse();
        result.Value.IsAcknowledged.ShouldBeTrue();
        result.Value.IsResolved.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithValidResolvedStatus_ShouldSucceed()
    {
        // Arrange & Act
        var result = AlertStatus.Create("Resolved");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(AlertStatus.Resolved);
        result.Value.IsPending.ShouldBeFalse();
        result.Value.IsAcknowledged.ShouldBeFalse();
        result.Value.IsResolved.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("PENDING")]
    [InlineData("pending")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithInvalidStatus_ShouldFail(string? invalidStatus)
    {
        // Arrange & Act
        var result = AlertStatus.Create(invalidStatus!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(e => e.Identifier == "AlertStatus.Invalid");
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void Pending_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var status = AlertStatus.Pending;

        // Assert
        status.Value.ShouldBe("Pending");
        status.IsPending.ShouldBeTrue();
    }

    [Fact]
    public void Acknowledged_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var status = AlertStatus.Acknowledged;

        // Assert
        status.Value.ShouldBe("Acknowledged");
        status.IsAcknowledged.ShouldBeTrue();
    }

    [Fact]
    public void Resolved_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var status = AlertStatus.Resolved;

        // Assert
        status.Value.ShouldBe("Resolved");
        status.IsResolved.ShouldBeTrue();
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_ShouldReturnAllStatuses()
    {
        // Arrange & Act
        var allStatuses = AlertStatus.GetAll().ToList();

        // Assert
        allStatuses.ShouldNotBeEmpty();
        allStatuses.Count.ShouldBe(3);
        allStatuses.ShouldContain(AlertStatus.Pending);
        allStatuses.ShouldContain(AlertStatus.Acknowledged);
        allStatuses.ShouldContain(AlertStatus.Resolved);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToString_ShouldWork()
    {
        // Arrange
        AlertStatus status = AlertStatus.Pending;

        // Act
        string stringValue = status;

        // Assert
        stringValue.ShouldBe("Pending");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equality_SameStatus_ShouldBeEqual()
    {
        // Arrange
        var status1 = AlertStatus.Pending;
        var status2 = AlertStatus.Pending;

        // Act & Assert
        (status1 == status2).ShouldBeTrue();
        status1.Equals(status2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentStatus_ShouldNotBeEqual()
    {
        // Arrange
        var status1 = AlertStatus.Pending;
        var status2 = AlertStatus.Acknowledged;

        // Act & Assert
        (status1 == status2).ShouldBeFalse();
        status1.Equals(status2).ShouldBeFalse();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var status = AlertStatus.Pending;

        // Act
        var result = status.ToString();

        // Assert
        result.ShouldBe("Pending");
    }

    #endregion
}
