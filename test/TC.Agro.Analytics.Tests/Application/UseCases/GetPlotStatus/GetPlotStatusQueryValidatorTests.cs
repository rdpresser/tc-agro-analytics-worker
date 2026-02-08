using TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

namespace TC.Agro.Analytics.Tests.Application.UseCases.GetPlotStatus;

/// <summary>
/// Unit tests for GetPlotStatusQueryValidator.
/// </summary>
public class GetPlotStatusQueryValidatorTests
{
    private readonly GetPlotStatusQueryValidator _validator;

    public GetPlotStatusQueryValidatorTests()
    {
        _validator = new GetPlotStatusQueryValidator();
    }

    [Fact]
    public void Validate_WithValidQuery_ShouldSucceed()
    {
        // Arrange
        var query = new GetPlotStatusQuery
        {
            PlotId = Guid.NewGuid()
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
        var query = new GetPlotStatusQuery
        {
            PlotId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetPlotStatusQuery.PlotId));
        result.Errors.ShouldContain(e => e.ErrorCode.Contains("Required"));
    }

    [Fact]
    public void Validate_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var query = new GetPlotStatusQuery
        {
            PlotId = new Guid("ae57f8d7-d491-4899-bb39-30124093e683")
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithNewGuid_ShouldSucceed()
    {
        // Arrange
        var plotId = Guid.NewGuid();
        var query = new GetPlotStatusQuery
        {
            PlotId = plotId
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
