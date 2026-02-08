using TC.Agro.Analytics.Application.UseCases.GetAlertHistory;
using TC.Agro.Analytics.Application.UseCases.Shared;

namespace TC.Agro.Analytics.Tests.Application.UseCases.GetAlertHistory;

/// <summary>
/// Unit tests for GetAlertHistoryQueryValidator.
/// </summary>
public class GetAlertHistoryQueryValidatorTests
{
    private readonly GetAlertHistoryQueryValidator _validator;

    public GetAlertHistoryQueryValidatorTests()
    {
        _validator = new GetAlertHistoryQueryValidator();
    }

    [Fact]
    public void Validate_WithValidQuery_ShouldSucceed()
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            PageNumber = 1,
            PageSize = 50
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithEmptyPlotId_ShouldFail()
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.Empty,
            Days = 30
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.PlotId));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("Required"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void Validate_WithInvalidDays_ShouldFail(int invalidDays)
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = invalidDays
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.Days));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("GreaterThanZero"));
    }

    [Fact]
    public void Validate_WithDaysExceeding365_ShouldFail()
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 366
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.Days));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("MaximumValue"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(365)]
    public void Validate_WithValidDays_ShouldSucceed(int validDays)
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = validDays
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithAlertTypeExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            AlertType = new string('A', 51) // > 50 characters
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.AlertType));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("MaximumLength"));
    }

    [Fact]
    public void Validate_WithStatusExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            Status = new string('A', 21) // > 20 characters
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.Status));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("MaximumLength"));
    }

    [Theory]
    [InlineData("HighTemperature")]
    [InlineData("LowSoilMoisture")]
    [InlineData("LowBattery")]
    [InlineData(null)]
    public void Validate_WithValidAlertType_ShouldSucceed(string? alertType)
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            AlertType = alertType
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Acknowledged")]
    [InlineData("Resolved")]
    [InlineData(null)]
    public void Validate_WithValidStatus_ShouldSucceed(string? status)
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            Status = status
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPageNumber_ShouldFail(int invalidPageNumber)
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            PageNumber = invalidPageNumber
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.PageNumber));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("GreaterThanZero"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPageSize_ShouldFail(int invalidPageSize)
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            PageSize = invalidPageSize
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.PageSize));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("GreaterThanZero"));
    }

    [Fact]
    public void Validate_WithPageSizeExceedingMaximum_ShouldFail()
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            PageSize = PaginationParams.MaxPageSize + 1
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetAlertHistoryQuery.PageSize));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("MaximumValue"));
    }

    [Fact]
    public void Validate_WithAllFiltersAndPagination_ShouldSucceed()
    {
        // Arrange
        var query = new GetAlertHistoryQuery
        {
            PlotId = Guid.NewGuid(),
            Days = 30,
            AlertType = "HighTemperature",
            Status = "Pending",
            PageNumber = 2,
            PageSize = 50
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
