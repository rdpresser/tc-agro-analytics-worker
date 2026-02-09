namespace TC.Agro.Analytics.Tests.Application.UseCases.GetPendingAlerts;

/// <summary>
/// Unit tests for GetPendingAlertsQueryValidator.
/// </summary>
public class GetPendingAlertsQueryValidatorTests
{
    private readonly GetPendingAlertsQueryValidator _validator;

    public GetPendingAlertsQueryValidatorTests()
    {
        _validator = new GetPendingAlertsQueryValidator();
    }

    [Fact]
    public void Validate_WithValidQuery_ShouldSucceed()
    {
        // Arrange
        var query = new GetPendingAlertsQuery
        {
            PageNumber = 1,
            PageSize = 50
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithDefaultValues_ShouldSucceed()
    {
        // Arrange
        var query = new GetPendingAlertsQuery(); // PageNumber = 1, PageSize = 100 (defaults)

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithInvalidPageNumber_ShouldFail(int invalidPageNumber)
    {
        // Arrange
        var query = new GetPendingAlertsQuery
        {
            PageNumber = invalidPageNumber,
            PageSize = 50
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsQuery.PageNumber));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("GreaterThanZero"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Validate_WithInvalidPageSize_ShouldFail(int invalidPageSize)
    {
        // Arrange
        var query = new GetPendingAlertsQuery
        {
            PageNumber = 1,
            PageSize = invalidPageSize
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsQuery.PageSize));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("GreaterThanZero"));
    }

    [Fact]
    public void Validate_WithPageSizeExceedingMaximum_ShouldFail()
    {
        // Arrange
        var query = new GetPendingAlertsQuery
        {
            PageNumber = 1,
            PageSize = PaginationParams.MaxPageSize + 1 // 501
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPendingAlertsQuery.PageSize));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("MaximumValue"));
    }

    [Fact]
    public void Validate_WithPageSizeAtMaximum_ShouldSucceed()
    {
        // Arrange
        var query = new GetPendingAlertsQuery
        {
            PageNumber = 1,
            PageSize = PaginationParams.MaxPageSize // 500
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 100)]
    [InlineData(5, 250)]
    [InlineData(100, 500)]
    public void Validate_WithValidPaginationCombinations_ShouldSucceed(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetPendingAlertsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
