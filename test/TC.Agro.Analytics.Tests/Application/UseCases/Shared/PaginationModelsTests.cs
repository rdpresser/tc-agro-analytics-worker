using TC.Agro.Analytics.Application.UseCases.Shared;

namespace TC.Agro.Analytics.Tests.Application.UseCases.Shared;

/// <summary>
/// Unit tests for PaginationParams and PagedResponse.
/// </summary>
public class PaginationModelsTests
{
    #region PaginationParams Tests

    [Fact]
    public void PaginationParams_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var pagination = new PaginationParams();

        // Assert
        pagination.PageNumber.ShouldBe(1);
        pagination.PageSize.ShouldBe(PaginationParams.DefaultPageSize);
        pagination.Skip.ShouldBe(0);
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 10, 20)]
    [InlineData(5, 25, 100)]
    [InlineData(10, 50, 450)]
    public void PaginationParams_Skip_ShouldCalculateCorrectly(int pageNumber, int pageSize, int expectedSkip)
    {
        // Arrange
        var pagination = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act
        var skip = pagination.Skip;

        // Assert
        skip.ShouldBe(expectedSkip);
    }

    [Fact]
    public void PaginationParams_MaxPageSize_ShouldBe500()
    {
        // Assert
        PaginationParams.MaxPageSize.ShouldBe(500);
    }

    [Fact]
    public void PaginationParams_DefaultPageSize_ShouldBe100()
    {
        // Assert
        PaginationParams.DefaultPageSize.ShouldBe(100);
    }

    #endregion

    #region PagedResponse Tests

    [Fact]
    public void PagedResponse_WithEmptyItems_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var response = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        response.Items.ShouldBeEmpty();
        response.TotalCount.ShouldBe(0);
        response.PageNumber.ShouldBe(1);
        response.PageSize.ShouldBe(10);
        response.TotalPages.ShouldBe(0);
        response.HasPreviousPage.ShouldBeFalse();
        response.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void PagedResponse_WithSinglePage_ShouldNotHavePreviousOrNext()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var response = new PagedResponse<int>
        {
            Items = items,
            TotalCount = 5,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        response.TotalPages.ShouldBe(1);
        response.HasPreviousPage.ShouldBeFalse();
        response.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void PagedResponse_OnFirstPage_ShouldHaveNextPageOnly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Act
        var response = new PagedResponse<int>
        {
            Items = items,
            TotalCount = 25,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        response.TotalPages.ShouldBe(3);
        response.HasPreviousPage.ShouldBeFalse();
        response.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public void PagedResponse_OnMiddlePage_ShouldHavePreviousAndNext()
    {
        // Arrange
        var items = new List<int> { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

        // Act
        var response = new PagedResponse<int>
        {
            Items = items,
            TotalCount = 30,
            PageNumber = 2,
            PageSize = 10
        };

        // Assert
        response.TotalPages.ShouldBe(3);
        response.HasPreviousPage.ShouldBeTrue();
        response.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public void PagedResponse_OnLastPage_ShouldHavePreviousPageOnly()
    {
        // Arrange
        var items = new List<int> { 21, 22, 23, 24, 25 };

        // Act
        var response = new PagedResponse<int>
        {
            Items = items,
            TotalCount = 25,
            PageNumber = 3,
            PageSize = 10
        };

        // Assert
        response.TotalPages.ShouldBe(3);
        response.HasPreviousPage.ShouldBeTrue();
        response.HasNextPage.ShouldBeFalse();
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(95, 10, 10)]
    [InlineData(101, 10, 11)]
    [InlineData(50, 25, 2)]
    [InlineData(75, 25, 3)]
    [InlineData(1, 10, 1)]
    [InlineData(0, 10, 0)]
    public void PagedResponse_TotalPages_ShouldCalculateCorrectly(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange & Act
        var response = new PagedResponse<int>
        {
            Items = [],
            TotalCount = totalCount,
            PageNumber = 1,
            PageSize = pageSize
        };

        // Assert
        response.TotalPages.ShouldBe(expectedTotalPages);
    }

    [Fact]
    public void PagedResponse_WithComplexType_ShouldWork()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new("Item 1"),
            new("Item 2"),
            new("Item 3")
        };

        // Act
        var response = new PagedResponse<TestItem>
        {
            Items = items,
            TotalCount = 10,
            PageNumber = 1,
            PageSize = 3
        };

        // Assert
        response.Items.Count.ShouldBe(3);
        response.TotalPages.ShouldBe(4);
        response.Items[0].Name.ShouldBe("Item 1");
    }

    private record TestItem(string Name);

    #endregion
}
