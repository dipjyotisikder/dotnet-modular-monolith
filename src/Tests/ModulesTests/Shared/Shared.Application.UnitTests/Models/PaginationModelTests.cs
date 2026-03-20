using Shared.Application.Models;


namespace Shared.Application.UnitTests.Models;

public class PaginationModelTests
{
    /// <summary>
    /// Tests that TotalPages property correctly calculates the number of pages
    /// for various combinations of TotalItems and PageSize values.
    /// </summary>
    /// <param name="totalItems">The total number of items.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="expectedTotalPages">The expected number of total pages.</param>
    [Theory]
    [InlineData(0, 10, 0)]           // Zero items should return 0 pages
    [InlineData(1, 10, 1)]           // Single item should return 1 page
    [InlineData(10, 10, 1)]          // Exact division should return correct pages
    [InlineData(11, 10, 2)]          // Ceiling: 11/10 = 1.1 -> 2 pages
    [InlineData(100, 10, 10)]        // Exact division: 100/10 = 10
    [InlineData(105, 10, 11)]        // Ceiling: 105/10 = 10.5 -> 11 pages
    [InlineData(5, 10, 1)]           // TotalItems < PageSize should return 1 page
    [InlineData(1, 1, 1)]            // Boundary: single item, single page size
    [InlineData(99, 10, 10)]         // 99/10 = 9.9 -> 10 pages
    [InlineData(101, 10, 11)]        // 101/10 = 10.1 -> 11 pages
    [InlineData(1000, 25, 40)]       // 1000/25 = 40
    [InlineData(1001, 25, 41)]       // 1001/25 = 40.04 -> 41 pages
    [InlineData(1, 100, 1)]          // Very large page size with few items
    [InlineData(10000, 1, 10000)]    // Large total items with page size 1
    public void TotalPages_ValidInputs_ReturnsCorrectPageCount(int totalItems, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        // Act
        var actualTotalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    /// <summary>
    /// Tests that TotalPages property handles division by zero when PageSize is 0 and TotalItems is 0.
    /// Expected: Returns 0 because (double)0 / 0 = NaN, and (int)Math.Ceiling(NaN) = 0.
    /// </summary>
    [Fact]
    public void TotalPages_ZeroPageSizeAndZeroTotalItems_ReturnsZero()
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = 0,
            PageSize = 0
        };

        // Act
        var totalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(0, totalPages);
    }

    /// <summary>
    /// Tests that TotalPages property handles division by zero when PageSize is 0 and TotalItems is positive.
    /// Expected: Returns int.MaxValue because (double)positive / 0 = +Infinity, and (int)Math.Ceiling(+Infinity) = int.MaxValue.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void TotalPages_ZeroPageSizeAndPositiveTotalItems_ReturnsIntMaxValue(int totalItems)
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = 0
        };

        // Act
        var totalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(int.MaxValue, totalPages);
    }

    /// <summary>
    /// Tests that TotalPages property handles division by zero when PageSize is 0 and TotalItems is negative.
    /// Expected: Returns int.MinValue because (double)negative / 0 = -Infinity, and (int)Math.Ceiling(-Infinity) = int.MinValue.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void TotalPages_ZeroPageSizeAndNegativeTotalItems_ReturnsIntMinValue(int totalItems)
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = 0
        };

        // Act
        var totalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(int.MinValue, totalPages);
    }

    /// <summary>
    /// Tests that TotalPages property handles negative TotalItems with positive PageSize.
    /// Expected: Returns negative or zero values based on ceiling calculation.
    /// </summary>
    [Theory]
    [InlineData(-1, 10, 0)]          // -1/10 = -0.1, Ceiling = 0
    [InlineData(-10, 10, -1)]        // -10/10 = -1.0, Ceiling = -1
    [InlineData(-11, 10, -1)]        // -11/10 = -1.1, Ceiling = -1
    [InlineData(-5, 10, 0)]          // -5/10 = -0.5, Ceiling = 0
    [InlineData(-100, 10, -10)]      // -100/10 = -10.0, Ceiling = -10
    public void TotalPages_NegativeTotalItemsAndPositivePageSize_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        // Act
        var actualTotalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    /// <summary>
    /// Tests that TotalPages property handles positive TotalItems with negative PageSize.
    /// Expected: Returns negative or zero values based on ceiling calculation.
    /// </summary>
    [Theory]
    [InlineData(1, -10, 0)]          // 1/-10 = -0.1, Ceiling = 0
    [InlineData(10, -10, -1)]        // 10/-10 = -1.0, Ceiling = -1
    [InlineData(11, -10, -1)]        // 11/-10 = -1.1, Ceiling = -1
    [InlineData(5, -10, 0)]          // 5/-10 = -0.5, Ceiling = 0
    [InlineData(100, -10, -10)]      // 100/-10 = -10.0, Ceiling = -10
    public void TotalPages_PositiveTotalItemsAndNegativePageSize_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        // Act
        var actualTotalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    /// <summary>
    /// Tests that TotalPages property handles both negative TotalItems and negative PageSize.
    /// Expected: Returns positive values when both are negative.
    /// </summary>
    [Theory]
    [InlineData(-1, -10, 1)]         // -1/-10 = 0.1, Ceiling = 1
    [InlineData(-10, -10, 1)]        // -10/-10 = 1.0, Ceiling = 1
    [InlineData(-11, -10, 2)]        // -11/-10 = 1.1, Ceiling = 2
    [InlineData(-5, -10, 1)]         // -5/-10 = 0.5, Ceiling = 1
    [InlineData(-100, -10, 10)]      // -100/-10 = 10.0, Ceiling = 10
    public void TotalPages_NegativeTotalItemsAndNegativePageSize_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        // Act
        var actualTotalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    /// <summary>
    /// Tests that TotalPages property correctly handles large values near integer boundaries.
    /// Expected: Returns correct calculated values without overflow issues.
    /// </summary>
    [Theory]
    [InlineData(int.MaxValue, 1, int.MaxValue)]           // Maximum items with page size 1
    [InlineData(int.MaxValue, int.MaxValue, 1)]           // Equal maximum values
    [InlineData(int.MaxValue, 1000000, 2148)]             // Large items with large page size
    [InlineData(int.MinValue, 1, int.MinValue)]           // Minimum items with page size 1
    [InlineData(int.MinValue, int.MinValue, 1)]           // Equal minimum values
    public void TotalPages_LargeOrExtremeBoundaryValues_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        // Act
        var actualTotalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    /// <summary>
    /// Tests that TotalPages property uses default values correctly when properties are not explicitly set.
    /// Expected: With default TotalItems=0 and PageSize=10, should return 0 pages.
    /// </summary>
    [Fact]
    public void TotalPages_DefaultValues_ReturnsZero()
    {
        // Arrange
        var pagination = new PaginationModel();

        // Act
        var totalPages = pagination.TotalPages;

        // Assert
        Assert.Equal(0, totalPages);
    }
}