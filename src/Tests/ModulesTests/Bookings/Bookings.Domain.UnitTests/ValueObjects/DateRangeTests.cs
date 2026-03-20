using Xunit;
using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.UnitTests.ValueObjects;

public class DateRangeTests
{
    /// <summary>
    /// Tests that OverlapsWith throws NullReferenceException when the other parameter is null.
    /// Input: null DateRange
    /// Expected: NullReferenceException
    /// </summary>
    [Fact]
    public void OverlapsWith_NullOtherParameter_ThrowsNullReferenceException()
    {
        // Arrange
        var checkIn = new DateTime(2024, 1, 1);
        var checkOut = new DateTime(2024, 1, 10);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;
        DateRange? other = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => dateRange.OverlapsWith(other!));
    }

    /// <summary>
    /// Tests that OverlapsWith correctly identifies when ranges do not overlap.
    /// Input: Various non-overlapping date ranges
    /// Expected: false
    /// </summary>
    [Theory]
    [InlineData("2024-01-01", "2024-01-05", "2024-01-10", "2024-01-15")] // This range completely before other
    [InlineData("2024-01-10", "2024-01-15", "2024-01-01", "2024-01-05")] // This range completely after other
    [InlineData("2024-01-01", "2024-01-05", "2024-01-05", "2024-01-10")] // Adjacent ranges (touching at boundary)
    [InlineData("2024-01-05", "2024-01-10", "2024-01-01", "2024-01-05")] // Adjacent ranges (reverse)
    public void OverlapsWith_NonOverlappingRanges_ReturnsFalse(
        string thisCheckIn, string thisCheckOut, string otherCheckIn, string otherCheckOut)
    {
        // Arrange
        var thisRange = DateRange.Create(DateTime.Parse(thisCheckIn), DateTime.Parse(thisCheckOut)).Value;
        var otherRange = DateRange.Create(DateTime.Parse(otherCheckIn), DateTime.Parse(otherCheckOut)).Value;

        // Act
        var result = thisRange.OverlapsWith(otherRange);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that OverlapsWith correctly identifies when ranges overlap.
    /// Input: Various overlapping date ranges
    /// Expected: true
    /// </summary>
    [Theory]
    [InlineData("2024-01-01", "2024-01-10", "2024-01-05", "2024-01-15")] // Partial overlap at end
    [InlineData("2024-01-05", "2024-01-15", "2024-01-01", "2024-01-10")] // Partial overlap at start
    [InlineData("2024-01-01", "2024-01-20", "2024-01-05", "2024-01-15")] // This range contains other
    [InlineData("2024-01-05", "2024-01-15", "2024-01-01", "2024-01-20")] // Other range contains this
    [InlineData("2024-01-01", "2024-01-10", "2024-01-01", "2024-01-10")] // Identical ranges
    [InlineData("2024-01-01", "2024-01-02", "2024-01-01", "2024-01-02")] // Single day ranges identical
    [InlineData("2024-01-01", "2024-01-10", "2024-01-02", "2024-01-09")] // Other completely inside this
    public void OverlapsWith_OverlappingRanges_ReturnsTrue(
        string thisCheckIn, string thisCheckOut, string otherCheckIn, string otherCheckOut)
    {
        // Arrange
        var thisRange = DateRange.Create(DateTime.Parse(thisCheckIn), DateTime.Parse(thisCheckOut)).Value;
        var otherRange = DateRange.Create(DateTime.Parse(otherCheckIn), DateTime.Parse(otherCheckOut)).Value;

        // Act
        var result = thisRange.OverlapsWith(otherRange);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests OverlapsWith with DateTime.MinValue as check-in date.
    /// Input: Date ranges starting with DateTime.MinValue
    /// Expected: Correct overlap detection
    /// </summary>
    [Fact]
    public void OverlapsWith_WithMinValueCheckIn_ReturnsExpectedResult()
    {
        // Arrange
        var minDate = DateTime.MinValue;
        var midDate = new DateTime(2024, 1, 15);
        var thisRange = DateRange.Create(minDate, midDate).Value;
        var otherRange = DateRange.Create(new DateTime(2024, 1, 1), new DateTime(2024, 1, 20)).Value;

        // Act
        var result = thisRange.OverlapsWith(otherRange);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests OverlapsWith with DateTime.MaxValue as check-out date.
    /// Input: Date ranges ending with DateTime.MaxValue
    /// Expected: Correct overlap detection
    /// </summary>
    [Fact]
    public void OverlapsWith_WithMaxValueCheckOut_ReturnsExpectedResult()
    {
        // Arrange
        var midDate = new DateTime(2024, 1, 1);
        var maxDate = DateTime.MaxValue;
        var thisRange = DateRange.Create(midDate, maxDate).Value;
        var otherRange = DateRange.Create(new DateTime(2023, 12, 1), new DateTime(2024, 1, 15)).Value;

        // Act
        var result = thisRange.OverlapsWith(otherRange);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests OverlapsWith with DateTime boundary values where ranges do not overlap.
    /// Input: One range at MinValue, another at MaxValue
    /// Expected: false
    /// </summary>
    [Fact]
    public void OverlapsWith_WithBoundaryValuesNoOverlap_ReturnsFalse()
    {
        // Arrange
        var midDate1 = new DateTime(2000, 1, 1);
        var midDate2 = new DateTime(3000, 1, 1);
        var thisRange = DateRange.Create(DateTime.MinValue, midDate1).Value;
        var otherRange = DateRange.Create(midDate2, DateTime.MaxValue).Value;

        // Act
        var result = thisRange.OverlapsWith(otherRange);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests OverlapsWith with very long date ranges spanning many years.
    /// Input: Date ranges spanning centuries
    /// Expected: Correct overlap detection
    /// </summary>
    [Fact]
    public void OverlapsWith_WithVeryLongRanges_ReturnsExpectedResult()
    {
        // Arrange
        var thisRange = DateRange.Create(new DateTime(1900, 1, 1), new DateTime(2100, 12, 31)).Value;
        var otherRange = DateRange.Create(new DateTime(2000, 1, 1), new DateTime(2200, 12, 31)).Value;

        // Act
        var result = thisRange.OverlapsWith(otherRange);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that Create returns a successful result with properly normalized dates
    /// when check-in date is before check-out date.
    /// </summary>
    /// <param name="checkInYear">Year for check-in date.</param>
    /// <param name="checkInMonth">Month for check-in date.</param>
    /// <param name="checkInDay">Day for check-in date.</param>
    /// <param name="checkInHour">Hour for check-in date.</param>
    /// <param name="checkInMinute">Minute for check-in date.</param>
    /// <param name="checkOutYear">Year for check-out date.</param>
    /// <param name="checkOutMonth">Month for check-out date.</param>
    /// <param name="checkOutDay">Day for check-out date.</param>
    /// <param name="checkOutHour">Hour for check-out date.</param>
    /// <param name="checkOutMinute">Minute for check-out date.</param>
    [Theory]
    [InlineData(2024, 1, 1, 0, 0, 2024, 1, 2, 0, 0)] // Adjacent dates
    [InlineData(2024, 1, 1, 14, 30, 2024, 1, 2, 8, 15)] // Different times, valid dates
    [InlineData(2024, 1, 1, 23, 59, 2024, 1, 2, 0, 0)] // Edge of day boundary
    [InlineData(2024, 1, 15, 12, 0, 2024, 2, 15, 12, 0)] // One month apart
    [InlineData(2024, 1, 1, 0, 0, 2024, 12, 31, 23, 59)] // Full year span
    public void Create_ValidCheckInBeforeCheckOut_ReturnsSuccessWithNormalizedDates(
        int checkInYear, int checkInMonth, int checkInDay, int checkInHour, int checkInMinute,
        int checkOutYear, int checkOutMonth, int checkOutDay, int checkOutHour, int checkOutMinute)
    {
        // Arrange
        var checkIn = new DateTime(checkInYear, checkInMonth, checkInDay, checkInHour, checkInMinute, 0);
        var checkOut = new DateTime(checkOutYear, checkOutMonth, checkOutDay, checkOutHour, checkOutMinute, 0);

        // Act
        var result = DateRange.Create(checkIn, checkOut);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(checkIn.Date, result.Value.CheckIn);
        Assert.Equal(checkOut.Date, result.Value.CheckOut);
        Assert.Equal(TimeSpan.Zero, result.Value.CheckIn.TimeOfDay);
        Assert.Equal(TimeSpan.Zero, result.Value.CheckOut.TimeOfDay);
    }

    /// <summary>
    /// Tests that Create returns a successful result when using DateTime.MinValue as check-in date.
    /// This validates the lower boundary of acceptable date range.
    /// </summary>
    [Fact]
    public void Create_MinValueAsCheckIn_ReturnsSuccess()
    {
        // Arrange
        var checkIn = DateTime.MinValue;
        var checkOut = DateTime.MinValue.AddDays(1);

        // Act
        var result = DateRange.Create(checkIn, checkOut);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(DateTime.MinValue.Date, result.Value.CheckIn);
        Assert.Equal(DateTime.MinValue.AddDays(1).Date, result.Value.CheckOut);
    }

    /// <summary>
    /// Tests that Create returns a successful result when using DateTime.MaxValue as check-out date.
    /// This validates the upper boundary of acceptable date range.
    /// </summary>
    [Fact]
    public void Create_MaxValueAsCheckOut_ReturnsSuccess()
    {
        // Arrange
        var checkIn = DateTime.MaxValue.AddDays(-1);
        var checkOut = DateTime.MaxValue;

        // Act
        var result = DateRange.Create(checkIn, checkOut);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(DateTime.MaxValue.AddDays(-1).Date, result.Value.CheckIn);
        Assert.Equal(DateTime.MaxValue.Date, result.Value.CheckOut);
    }

    /// <summary>
    /// Tests that Create returns a failure result with appropriate error message and code
    /// when check-in date is the same as check-out date.
    /// </summary>
    /// <param name="year">Year for the date.</param>
    /// <param name="month">Month for the date.</param>
    /// <param name="day">Day for the date.</param>
    /// <param name="checkInHour">Hour for check-in.</param>
    /// <param name="checkInMinute">Minute for check-in.</param>
    /// <param name="checkOutHour">Hour for check-out.</param>
    /// <param name="checkOutMinute">Minute for check-out.</param>
    [Theory]
    [InlineData(2024, 1, 15, 0, 0, 0, 0)] // Same date and time
    [InlineData(2024, 1, 15, 8, 0, 14, 30)] // Same date, different times
    [InlineData(2024, 1, 15, 23, 59, 0, 0)] // Same date, reversed times
    public void Create_CheckInSameDateAsCheckOut_ReturnsFailure(
        int year, int month, int day,
        int checkInHour, int checkInMinute,
        int checkOutHour, int checkOutMinute)
    {
        // Arrange
        var checkIn = new DateTime(year, month, day, checkInHour, checkInMinute, 0);
        var checkOut = new DateTime(year, month, day, checkOutHour, checkOutMinute, 0);

        // Act
        var result = DateRange.Create(checkIn, checkOut);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create returns a failure result when check-in date is after check-out date.
    /// This validates that date order is properly enforced.
    /// </summary>
    /// <param name="checkInYear">Year for check-in date.</param>
    /// <param name="checkInMonth">Month for check-in date.</param>
    /// <param name="checkInDay">Day for check-in date.</param>
    /// <param name="checkOutYear">Year for check-out date.</param>
    /// <param name="checkOutMonth">Month for check-out date.</param>
    /// <param name="checkOutDay">Day for check-out date.</param>
    [Theory]
    [InlineData(2024, 1, 2, 2024, 1, 1)] // One day reversed
    [InlineData(2024, 2, 1, 2024, 1, 31)] // One month reversed
    [InlineData(2024, 12, 31, 2024, 1, 1)] // Full year reversed
    [InlineData(2025, 1, 1, 2024, 12, 31)] // Year boundary reversed
    public void Create_CheckInAfterCheckOut_ReturnsFailure(
        int checkInYear, int checkInMonth, int checkInDay,
        int checkOutYear, int checkOutMonth, int checkOutDay)
    {
        // Arrange
        var checkIn = new DateTime(checkInYear, checkInMonth, checkInDay);
        var checkOut = new DateTime(checkOutYear, checkOutMonth, checkOutDay);

        // Act
        var result = DateRange.Create(checkIn, checkOut);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create returns a failure result when both dates are DateTime.MinValue.
    /// This validates boundary condition for equal dates at the minimum value.
    /// </summary>
    [Fact]
    public void Create_BothDatesMinValue_ReturnsFailure()
    {
        // Arrange
        var checkIn = DateTime.MinValue;
        var checkOut = DateTime.MinValue;

        // Act
        var result = DateRange.Create(checkIn, checkOut);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create returns a failure result when both dates are DateTime.MaxValue.
    /// This validates boundary condition for equal dates at the maximum value.
    /// </summary>
    [Fact]
    public void Create_BothDatesMaxValue_ReturnsFailure()
    {
        // Arrange
        var checkIn = DateTime.MaxValue;
        var checkOut = DateTime.MaxValue;

        // Act
        var result = DateRange.Create(checkIn, checkOut);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that the Nights property correctly calculates the number of nights
    /// between check-in and check-out dates for various date ranges.
    /// </summary>
    /// <param name="checkInYear">Check-in year</param>
    /// <param name="checkInMonth">Check-in month</param>
    /// <param name="checkInDay">Check-in day</param>
    /// <param name="checkOutYear">Check-out year</param>
    /// <param name="checkOutMonth">Check-out month</param>
    /// <param name="checkOutDay">Check-out day</param>
    /// <param name="expectedNights">Expected number of nights</param>
    [Theory]
    [InlineData(2024, 1, 1, 2024, 1, 2, 1)]           // One night
    [InlineData(2024, 1, 1, 2024, 1, 8, 7)]           // One week
    [InlineData(2024, 1, 1, 2024, 1, 31, 30)]         // One month (30 nights)
    [InlineData(2024, 1, 1, 2024, 2, 1, 31)]          // 31 nights
    [InlineData(2024, 1, 1, 2025, 1, 1, 366)]         // One year (leap year)
    [InlineData(2023, 1, 1, 2024, 1, 1, 365)]         // One year (non-leap year)
    [InlineData(2024, 2, 28, 2024, 3, 1, 2)]          // Across leap year boundary
    [InlineData(2023, 2, 28, 2023, 3, 1, 1)]          // Across non-leap year boundary
    [InlineData(2024, 12, 31, 2025, 1, 1, 1)]         // Year boundary
    [InlineData(2000, 1, 1, 2024, 1, 1, 8766)]        // Large range (24 years)
    public void Nights_ValidDateRange_ReturnsCorrectNumberOfNights(
        int checkInYear, int checkInMonth, int checkInDay,
        int checkOutYear, int checkOutMonth, int checkOutDay,
        int expectedNights)
    {
        // Arrange
        var checkIn = new DateTime(checkInYear, checkInMonth, checkInDay);
        var checkOut = new DateTime(checkOutYear, checkOutMonth, checkOutDay);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        // Act
        var nights = dateRange.Nights;

        // Assert
        Assert.Equal(expectedNights, nights);
    }

    /// <summary>
    /// Tests that the Nights property correctly calculates nights when check-in
    /// and check-out times differ but dates are consecutive, ensuring time
    /// components are properly ignored.
    /// </summary>
    /// <param name="checkInHour">Check-in hour</param>
    /// <param name="checkOutHour">Check-out hour</param>
    [Theory]
    [InlineData(0, 0)]      // Midnight to midnight
    [InlineData(15, 11)]    // 3 PM to 11 AM (different times, same date difference)
    [InlineData(23, 0)]     // 11 PM to midnight
    [InlineData(0, 23)]     // Midnight to 11 PM
    [InlineData(12, 12)]    // Noon to noon
    public void Nights_DifferentTimesOnConsecutiveDates_ReturnsOneNight(
        int checkInHour, int checkOutHour)
    {
        // Arrange
        var checkIn = new DateTime(2024, 6, 15, checkInHour, 30, 45);
        var checkOut = new DateTime(2024, 6, 16, checkOutHour, 15, 20);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        // Act
        var nights = dateRange.Nights;

        // Assert
        Assert.Equal(1, nights);
    }

    /// <summary>
    /// Tests that the Nights property correctly handles date ranges near
    /// DateTime.MinValue, ensuring no overflow or unexpected behavior occurs.
    /// </summary>
    [Fact]
    public void Nights_DateRangeNearMinValue_ReturnsCorrectNights()
    {
        // Arrange
        var checkIn = DateTime.MinValue;
        var checkOut = DateTime.MinValue.AddDays(10);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        // Act
        var nights = dateRange.Nights;

        // Assert
        Assert.Equal(10, nights);
    }

    /// <summary>
    /// Tests that the Nights property correctly handles date ranges near
    /// DateTime.MaxValue, ensuring no overflow or unexpected behavior occurs.
    /// </summary>
    [Fact]
    public void Nights_DateRangeNearMaxValue_ReturnsCorrectNights()
    {
        // Arrange
        var checkOut = DateTime.MaxValue.Date;
        var checkIn = checkOut.AddDays(-10);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        // Act
        var nights = dateRange.Nights;

        // Assert
        Assert.Equal(10, nights);
    }
}