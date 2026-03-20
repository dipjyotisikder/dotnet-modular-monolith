using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.UnitTests.ValueObjects;

public class DateRangeTests
{
    [Fact]
    public void OverlapsWith_NullOtherParameter_ThrowsNullReferenceException()
    {
        var checkIn = new DateTime(2024, 1, 1);
        var checkOut = new DateTime(2024, 1, 10);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;
        DateRange? other = null;

        Assert.Throws<NullReferenceException>(() => dateRange.OverlapsWith(other!));
    }

    [Theory]
    [InlineData("2024-01-01", "2024-01-05", "2024-01-10", "2024-01-15")]
    [InlineData("2024-01-10", "2024-01-15", "2024-01-01", "2024-01-05")]
    [InlineData("2024-01-01", "2024-01-05", "2024-01-05", "2024-01-10")]
    [InlineData("2024-01-05", "2024-01-10", "2024-01-01", "2024-01-05")]
    public void OverlapsWith_NonOverlappingRanges_ReturnsFalse(
        string thisCheckIn, string thisCheckOut, string otherCheckIn, string otherCheckOut)
    {
        var thisRange = DateRange.Create(DateTime.Parse(thisCheckIn), DateTime.Parse(thisCheckOut)).Value;
        var otherRange = DateRange.Create(DateTime.Parse(otherCheckIn), DateTime.Parse(otherCheckOut)).Value;

        var result = thisRange.OverlapsWith(otherRange);

        Assert.False(result);
    }

    [Theory]
    [InlineData("2024-01-01", "2024-01-10", "2024-01-05", "2024-01-15")]
    [InlineData("2024-01-05", "2024-01-15", "2024-01-01", "2024-01-10")]
    [InlineData("2024-01-01", "2024-01-20", "2024-01-05", "2024-01-15")]
    [InlineData("2024-01-05", "2024-01-15", "2024-01-01", "2024-01-20")]
    [InlineData("2024-01-01", "2024-01-10", "2024-01-01", "2024-01-10")]
    [InlineData("2024-01-01", "2024-01-02", "2024-01-01", "2024-01-02")]
    [InlineData("2024-01-01", "2024-01-10", "2024-01-02", "2024-01-09")]
    public void OverlapsWith_OverlappingRanges_ReturnsTrue(
        string thisCheckIn, string thisCheckOut, string otherCheckIn, string otherCheckOut)
    {
        var thisRange = DateRange.Create(DateTime.Parse(thisCheckIn), DateTime.Parse(thisCheckOut)).Value;
        var otherRange = DateRange.Create(DateTime.Parse(otherCheckIn), DateTime.Parse(otherCheckOut)).Value;

        var result = thisRange.OverlapsWith(otherRange);

        Assert.True(result);
    }

    [Fact]
    public void OverlapsWith_WithMinValueCheckIn_ReturnsExpectedResult()
    {
        var minDate = DateTime.MinValue;
        var midDate = new DateTime(2024, 1, 15);
        var thisRange = DateRange.Create(minDate, midDate).Value;
        var otherRange = DateRange.Create(new DateTime(2024, 1, 1), new DateTime(2024, 1, 20)).Value;

        var result = thisRange.OverlapsWith(otherRange);

        Assert.True(result);
    }

    [Fact]
    public void OverlapsWith_WithMaxValueCheckOut_ReturnsExpectedResult()
    {
        var midDate = new DateTime(2024, 1, 1);
        var maxDate = DateTime.MaxValue;
        var thisRange = DateRange.Create(midDate, maxDate).Value;
        var otherRange = DateRange.Create(new DateTime(2023, 12, 1), new DateTime(2024, 1, 15)).Value;

        var result = thisRange.OverlapsWith(otherRange);

        Assert.True(result);
    }

    [Fact]
    public void OverlapsWith_WithBoundaryValuesNoOverlap_ReturnsFalse()
    {
        var midDate1 = new DateTime(2000, 1, 1);
        var midDate2 = new DateTime(3000, 1, 1);
        var thisRange = DateRange.Create(DateTime.MinValue, midDate1).Value;
        var otherRange = DateRange.Create(midDate2, DateTime.MaxValue).Value;

        var result = thisRange.OverlapsWith(otherRange);

        Assert.False(result);
    }

    [Fact]
    public void OverlapsWith_WithVeryLongRanges_ReturnsExpectedResult()
    {
        var thisRange = DateRange.Create(new DateTime(1900, 1, 1), new DateTime(2100, 12, 31)).Value;
        var otherRange = DateRange.Create(new DateTime(2000, 1, 1), new DateTime(2200, 12, 31)).Value;

        var result = thisRange.OverlapsWith(otherRange);

        Assert.True(result);
    }

    [Theory]
    [InlineData(2024, 1, 1, 0, 0, 2024, 1, 2, 0, 0)]
    [InlineData(2024, 1, 1, 14, 30, 2024, 1, 2, 8, 15)]
    [InlineData(2024, 1, 1, 23, 59, 2024, 1, 2, 0, 0)]
    [InlineData(2024, 1, 15, 12, 0, 2024, 2, 15, 12, 0)]
    [InlineData(2024, 1, 1, 0, 0, 2024, 12, 31, 23, 59)]
    public void Create_ValidCheckInBeforeCheckOut_ReturnsSuccessWithNormalizedDates(
        int checkInYear, int checkInMonth, int checkInDay, int checkInHour, int checkInMinute,
        int checkOutYear, int checkOutMonth, int checkOutDay, int checkOutHour, int checkOutMinute)
    {
        var checkIn = new DateTime(checkInYear, checkInMonth, checkInDay, checkInHour, checkInMinute, 0);
        var checkOut = new DateTime(checkOutYear, checkOutMonth, checkOutDay, checkOutHour, checkOutMinute, 0);

        var result = DateRange.Create(checkIn, checkOut);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(checkIn.Date, result.Value.CheckIn);
        Assert.Equal(checkOut.Date, result.Value.CheckOut);
        Assert.Equal(TimeSpan.Zero, result.Value.CheckIn.TimeOfDay);
        Assert.Equal(TimeSpan.Zero, result.Value.CheckOut.TimeOfDay);
    }

    [Fact]
    public void Create_MinValueAsCheckIn_ReturnsSuccess()
    {
        var checkIn = DateTime.MinValue;
        var checkOut = DateTime.MinValue.AddDays(1);

        var result = DateRange.Create(checkIn, checkOut);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(DateTime.MinValue.Date, result.Value.CheckIn);
        Assert.Equal(DateTime.MinValue.AddDays(1).Date, result.Value.CheckOut);
    }

    [Fact]
    public void Create_MaxValueAsCheckOut_ReturnsSuccess()
    {
        var checkIn = DateTime.MaxValue.AddDays(-1);
        var checkOut = DateTime.MaxValue;

        var result = DateRange.Create(checkIn, checkOut);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(DateTime.MaxValue.AddDays(-1).Date, result.Value.CheckIn);
        Assert.Equal(DateTime.MaxValue.Date, result.Value.CheckOut);
    }

    [Theory]
    [InlineData(2024, 1, 15, 0, 0, 0, 0)]
    [InlineData(2024, 1, 15, 8, 0, 14, 30)]
    [InlineData(2024, 1, 15, 23, 59, 0, 0)]
    public void Create_CheckInSameDateAsCheckOut_ReturnsFailure(
        int year, int month, int day,
        int checkInHour, int checkInMinute,
        int checkOutHour, int checkOutMinute)
    {
        var checkIn = new DateTime(year, month, day, checkInHour, checkInMinute, 0);
        var checkOut = new DateTime(year, month, day, checkOutHour, checkOutMinute, 0);

        var result = DateRange.Create(checkIn, checkOut);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(2024, 1, 2, 2024, 1, 1)]
    [InlineData(2024, 2, 1, 2024, 1, 31)]
    [InlineData(2024, 12, 31, 2024, 1, 1)]
    [InlineData(2025, 1, 1, 2024, 12, 31)]
    public void Create_CheckInAfterCheckOut_ReturnsFailure(
        int checkInYear, int checkInMonth, int checkInDay,
        int checkOutYear, int checkOutMonth, int checkOutDay)
    {
        var checkIn = new DateTime(checkInYear, checkInMonth, checkInDay);
        var checkOut = new DateTime(checkOutYear, checkOutMonth, checkOutDay);

        var result = DateRange.Create(checkIn, checkOut);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_BothDatesMinValue_ReturnsFailure()
    {
        var checkIn = DateTime.MinValue;
        var checkOut = DateTime.MinValue;

        var result = DateRange.Create(checkIn, checkOut);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_BothDatesMaxValue_ReturnsFailure()
    {
        var checkIn = DateTime.MaxValue;
        var checkOut = DateTime.MaxValue;

        var result = DateRange.Create(checkIn, checkOut);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Check-out date must be after check-in date", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(2024, 1, 1, 2024, 1, 2, 1)]
    [InlineData(2024, 1, 1, 2024, 1, 8, 7)]
    [InlineData(2024, 1, 1, 2024, 1, 31, 30)]
    [InlineData(2024, 1, 1, 2024, 2, 1, 31)]
    [InlineData(2024, 1, 1, 2025, 1, 1, 366)]
    [InlineData(2023, 1, 1, 2024, 1, 1, 365)]
    [InlineData(2024, 2, 28, 2024, 3, 1, 2)]
    [InlineData(2023, 2, 28, 2023, 3, 1, 1)]
    [InlineData(2024, 12, 31, 2025, 1, 1, 1)]
    [InlineData(2000, 1, 1, 2024, 1, 1, 8766)]
    public void Nights_ValidDateRange_ReturnsCorrectNumberOfNights(
        int checkInYear, int checkInMonth, int checkInDay,
        int checkOutYear, int checkOutMonth, int checkOutDay,
        int expectedNights)
    {
        var checkIn = new DateTime(checkInYear, checkInMonth, checkInDay);
        var checkOut = new DateTime(checkOutYear, checkOutMonth, checkOutDay);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        var nights = dateRange.Nights;

        Assert.Equal(expectedNights, nights);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(15, 11)]
    [InlineData(23, 0)]
    [InlineData(0, 23)]
    [InlineData(12, 12)]
    public void Nights_DifferentTimesOnConsecutiveDates_ReturnsOneNight(
        int checkInHour, int checkOutHour)
    {
        var checkIn = new DateTime(2024, 6, 15, checkInHour, 30, 45);
        var checkOut = new DateTime(2024, 6, 16, checkOutHour, 15, 20);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        var nights = dateRange.Nights;

        Assert.Equal(1, nights);
    }

    [Fact]
    public void Nights_DateRangeNearMinValue_ReturnsCorrectNights()
    {
        var checkIn = DateTime.MinValue;
        var checkOut = DateTime.MinValue.AddDays(10);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        var nights = dateRange.Nights;

        Assert.Equal(10, nights);
    }

    [Fact]
    public void Nights_DateRangeNearMaxValue_ReturnsCorrectNights()
    {
        var checkOut = DateTime.MaxValue.Date;
        var checkIn = checkOut.AddDays(-10);
        var result = DateRange.Create(checkIn, checkOut);
        var dateRange = result.Value;

        var nights = dateRange.Nights;

        Assert.Equal(10, nights);
    }
}
