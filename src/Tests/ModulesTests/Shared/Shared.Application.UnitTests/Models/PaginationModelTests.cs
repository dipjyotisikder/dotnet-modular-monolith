using Shared.Application.Models;

namespace Shared.Application.UnitTests.Models;

public class PaginationModelTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(100, 10, 10)]
    [InlineData(105, 10, 11)]
    [InlineData(5, 10, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(99, 10, 10)]
    [InlineData(101, 10, 11)]
    [InlineData(1000, 25, 40)]
    [InlineData(1001, 25, 41)]
    [InlineData(1, 100, 1)]
    [InlineData(10000, 1, 10000)]
    public void TotalPages_ValidInputs_ReturnsCorrectPageCount(int totalItems, int pageSize, int expectedTotalPages)
    {
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        var actualTotalPages = pagination.TotalPages;

        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    [Fact]
    public void TotalPages_ZeroPageSizeAndZeroTotalItems_ReturnsZero()
    {
        var pagination = new PaginationModel
        {
            TotalItems = 0,
            PageSize = 0
        };

        var totalPages = pagination.TotalPages;

        Assert.Equal(0, totalPages);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void TotalPages_ZeroPageSizeAndPositiveTotalItems_ReturnsIntMaxValue(int totalItems)
    {
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = 0
        };

        var totalPages = pagination.TotalPages;

        Assert.Equal(int.MaxValue, totalPages);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void TotalPages_ZeroPageSizeAndNegativeTotalItems_ReturnsIntMinValue(int totalItems)
    {
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = 0
        };

        var totalPages = pagination.TotalPages;

        Assert.Equal(int.MinValue, totalPages);
    }

    [Theory]
    [InlineData(-1, 10, 0)]
    [InlineData(-10, 10, -1)]
    [InlineData(-11, 10, -1)]
    [InlineData(-5, 10, 0)]
    [InlineData(-100, 10, -10)]
    public void TotalPages_NegativeTotalItemsAndPositivePageSize_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        var actualTotalPages = pagination.TotalPages;

        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    [Theory]
    [InlineData(1, -10, 0)]
    [InlineData(10, -10, -1)]
    [InlineData(11, -10, -1)]
    [InlineData(5, -10, 0)]
    [InlineData(100, -10, -10)]
    public void TotalPages_PositiveTotalItemsAndNegativePageSize_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        var actualTotalPages = pagination.TotalPages;

        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    [Theory]
    [InlineData(-1, -10, 1)]
    [InlineData(-10, -10, 1)]
    [InlineData(-11, -10, 2)]
    [InlineData(-5, -10, 1)]
    [InlineData(-100, -10, 10)]
    public void TotalPages_NegativeTotalItemsAndNegativePageSize_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        var actualTotalPages = pagination.TotalPages;

        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    [Theory]
    [InlineData(int.MaxValue, 1, int.MaxValue)]
    [InlineData(int.MaxValue, int.MaxValue, 1)]
    [InlineData(int.MaxValue, 1000000, 2148)]
    [InlineData(int.MinValue, 1, int.MinValue)]
    [InlineData(int.MinValue, int.MinValue, 1)]
    public void TotalPages_LargeOrExtremeBoundaryValues_ReturnsCorrectValue(int totalItems, int pageSize, int expectedTotalPages)
    {
        var pagination = new PaginationModel
        {
            TotalItems = totalItems,
            PageSize = pageSize
        };

        var actualTotalPages = pagination.TotalPages;

        Assert.Equal(expectedTotalPages, actualTotalPages);
    }

    [Fact]
    public void TotalPages_DefaultValues_ReturnsZero()
    {
        var pagination = new PaginationModel();

        var totalPages = pagination.TotalPages;

        Assert.Equal(0, totalPages);
    }
}
