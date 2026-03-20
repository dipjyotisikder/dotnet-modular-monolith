using Bookings.Domain.Entities;
using Bookings.Domain.Enums;
using Bookings.Domain.Events;
using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.UnitTests.Entities;

public class BookingTests
{
    [Fact]
    public void BelongsTo_WhenUserIdMatchesGuestId_ReturnsTrue()
    {
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)).Value;
        var money = Money.Create(100m, "USD").Value;

        var bookingResult = Booking.Create(guestId, hotelId, roomId, dateRange, money);
        var booking = bookingResult.Value;

        var result = booking.BelongsTo(guestId);

        Assert.True(result);
    }

    [Fact]
    public void BelongsTo_WhenUserIdDoesNotMatchGuestId_ReturnsFalse()
    {
        var guestId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)).Value;
        var money = Money.Create(100m, "USD").Value;

        var bookingResult = Booking.Create(guestId, hotelId, roomId, dateRange, money);
        var booking = bookingResult.Value;

        var result = booking.BelongsTo(differentUserId);

        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BelongsTo_WithVariousUserIds_ReturnsExpectedResult(bool shouldMatch)
    {
        var guestId = Guid.NewGuid();
        var userId = shouldMatch ? guestId : Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)).Value;
        var money = Money.Create(100m, "USD").Value;

        var bookingResult = Booking.Create(guestId, hotelId, roomId, dateRange, money);
        var booking = bookingResult.Value;

        var result = booking.BelongsTo(userId);

        Assert.Equal(shouldMatch, result);
    }

    [Fact]
    public void Complete_WhenStatusIsConfirmed_ShouldSucceedAndChangeStatusToCompleted()
    {
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;
        booking.ClearDomainEvents();

        var result = booking.Complete();

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.Single(booking.DomainEvents);
        var domainEvent = booking.DomainEvents.First() as BookingCompletedDomainEvent;
        Assert.NotNull(domainEvent);
        Assert.Equal(booking.Id, domainEvent.BookingId);
        Assert.Equal(guestId, domainEvent.GuestId);
        Assert.Equal(hotelId, domainEvent.HotelId);
    }

    [Theory]
    [InlineData(BookingStatus.Cancelled, "Cancel")]
    [InlineData(BookingStatus.Completed, "Complete")]
    public void Complete_WhenStatusIsNotConfirmed_ShouldReturnFailure(BookingStatus statusToSet, string setupAction)
    {
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;

        if (setupAction == "Cancel")
        {
            booking.Cancel();
        }
        else if (setupAction == "Complete")
        {
            booking.Complete();
        }

        booking.ClearDomainEvents();
        var eventCountBeforeAct = booking.DomainEvents.Count;

        var result = booking.Complete();

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Only confirmed bookings can be completed", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
        Assert.Equal(statusToSet, booking.Status);
        Assert.Equal(eventCountBeforeAct, booking.DomainEvents.Count);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFailureAndNotAddDuplicateEvent()
    {
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;
        booking.Complete();
        booking.ClearDomainEvents();

        var result = booking.Complete();

        Assert.False(result.IsSuccess);
        Assert.Equal("Only confirmed bookings can be completed", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.Empty(booking.DomainEvents);
    }

    [Fact]
    public void Complete_WhenStatusIsUndefinedEnumValue_ShouldReturnFailure()
    {
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;

        var statusProperty = typeof(Booking).GetProperty("Status");
        statusProperty?.SetValue(booking, (BookingStatus)999);

        var result = booking.Complete();

        Assert.False(result.IsSuccess);
        Assert.Equal("Only confirmed bookings can be completed", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    [Fact]
    public void Cancel_WhenBookingAlreadyCancelled_ReturnsFailure()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        var firstCancelResult = booking.Cancel("First cancellation");

        var result = booking.Cancel("Second cancellation attempt");

        Assert.True(firstCancelResult.IsSuccess);
        Assert.False(result.IsSuccess);
        Assert.Equal("Booking is already cancelled", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    [Fact]
    public void Cancel_WhenBookingCompleted_ReturnsFailure()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: -5);
        booking.Complete();

        var result = booking.Cancel("Trying to cancel completed booking");

        Assert.False(result.IsSuccess);
        Assert.Equal("Completed bookings cannot be cancelled", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    [Fact]
    public void Cancel_WhenCheckInDateIsToday_ReturnsFailure()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 0);

        var result = booking.Cancel("Trying to cancel on check-in day");

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot cancel a booking on or after the check-in date", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void Cancel_WhenCheckInDateIsInPast_ReturnsFailure(int daysInPast)
    {
        var booking = CreateValidBooking(checkInDaysFromNow: daysInPast);

        var result = booking.Cancel("Trying to cancel past booking");

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot cancel a booking on or after the check-in date", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(30)]
    [InlineData(365)]
    public void Cancel_WithValidFutureCheckInDate_ReturnsSuccess(int daysFromNow)
    {
        var booking = CreateValidBooking(checkInDaysFromNow: daysFromNow);
        var beforeCancel = DateTime.UtcNow;

        var result = booking.Cancel("Custom cancellation reason");

        var afterCancel = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.CancelledAt);
        Assert.True(booking.CancelledAt >= beforeCancel && booking.CancelledAt <= afterCancel);
        Assert.Equal("Custom cancellation reason", booking.CancellationReason);
        Assert.Single(booking.DomainEvents);
        Assert.IsType<BookingCancelledDomainEvent>(booking.DomainEvents[0]);
    }

    [Fact]
    public void Cancel_WithoutReason_UsesDefaultReason()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 5);

        var result = booking.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal("Cancelled by guest", booking.CancellationReason);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    [InlineData("A")]
    [InlineData("Normal cancellation reason")]
    [InlineData("Special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [InlineData("Unicode: 日本語 Español Français")]
    public void Cancel_WithVariousReasonFormats_AcceptsReason(string reason)
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 5);

        var result = booking.Cancel(reason);

        Assert.True(result.IsSuccess);
        Assert.Equal(reason, booking.CancellationReason);
    }

    [Fact]
    public void Cancel_WithVeryLongReason_AcceptsReason()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        var longReason = new string('A', 10000);

        var result = booking.Cancel(longReason);

        Assert.True(result.IsSuccess);
        Assert.Equal(longReason, booking.CancellationReason);
    }

    [Fact]
    public void Cancel_WhenSuccessful_AddsDomainEventWithCorrectProperties()
    {
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var booking = CreateValidBooking(guestId, hotelId, roomId, checkInDaysFromNow: 5);
        var reason = "Test cancellation reason";

        var result = booking.Cancel(reason);

        Assert.True(result.IsSuccess);
        Assert.Single(booking.DomainEvents);
        var domainEvent = Assert.IsType<BookingCancelledDomainEvent>(booking.DomainEvents[0]);
        Assert.Equal(booking.Id, domainEvent.BookingId);
        Assert.Equal(guestId, domainEvent.GuestId);
        Assert.Equal(hotelId, domainEvent.HotelId);
        Assert.Equal(roomId, domainEvent.RoomId);
        Assert.Equal(reason, domainEvent.Reason);
    }

    [Fact]
    public void Cancel_WhenSuccessful_SetsCancelledAtToCurrentUtcTime()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        var beforeCancel = DateTime.UtcNow;

        var result = booking.Cancel();

        var afterCancel = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.NotNull(booking.CancelledAt);
        Assert.True(booking.CancelledAt.Value >= beforeCancel);
        Assert.True(booking.CancelledAt.Value <= afterCancel);
    }

    [Fact]
    public void Cancel_BeforeCancellation_HasNullCancelledAtAndReason()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 5);

        Assert.Null(booking.CancelledAt);
        Assert.Null(booking.CancellationReason);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Cancel_WhenSuccessful_ChangesStatusFromConfirmedToCancelled()
    {
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);

        var result = booking.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    private Booking CreateValidBooking(int checkInDaysFromNow, int nights = 3)
    {
        return CreateValidBooking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), checkInDaysFromNow, nights);
    }

    private Booking CreateValidBooking(Guid guestId, Guid hotelId, Guid roomId, int checkInDaysFromNow, int nights = 3)
    {
        var checkIn = DateTime.UtcNow.Date.AddDays(checkInDaysFromNow);
        var checkOut = checkIn.AddDays(nights);
        var dateRange = DateRange.Create(checkIn, checkOut).Value;
        var pricePerNight = Money.Create(100m, "USD").Value;

        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;
        booking.ClearDomainEvents();

        return booking;
    }

    [Fact]
    public void Create_EmptyGuestId_ReturnsFailureResult()
    {
        Guid guestId = Guid.Empty;
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Guest ID cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_EmptyHotelId_ReturnsFailureResult()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.Empty;
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Hotel ID cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_EmptyRoomId_ReturnsFailureResult()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.Empty;
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Room ID cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_ValidInputs_ReturnsSuccessResultWithBooking()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(3);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(150m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(guestId, result.Value.GuestId);
        Assert.Equal(hotelId, result.Value.HotelId);
        Assert.Equal(roomId, result.Value.RoomId);
        Assert.Equal(dateRange, result.Value.DateRange);
        Assert.Equal(BookingStatus.Confirmed, result.Value.Status);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(100, 1, 100)]
    [InlineData(100, 2, 200)]
    [InlineData(100, 5, 500)]
    [InlineData(50.50, 3, 151.50)]
    [InlineData(0, 5, 0)]
    [InlineData(1, 1, 1)]
    public void Create_VariousPricesAndNights_CalculatesTotalAmountCorrectly(decimal priceAmount, int nights, decimal expectedTotal)
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(nights);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(priceAmount).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTotal, result.Value.TotalAmount.Amount);
    }

    [Fact]
    public void Create_ValidInputs_SetsCreatedAtToCurrentUtcTime()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;
        DateTime beforeCreate = DateTime.UtcNow;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);
        DateTime afterCreate = DateTime.UtcNow;

        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.CreatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public void Create_ValidInputs_InitializesCancelledAtAsNull()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.CancelledAt);
    }

    [Fact]
    public void Create_ValidInputs_InitializesCancellationReasonAsNull()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.CancellationReason);
    }

    [Fact]
    public void Create_ValidInputs_GeneratesUniqueBookingId()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        Result<Booking> result1 = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);
        Result<Booking> result2 = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(Guid.Empty, result1.Value.Id);
        Assert.NotEqual(Guid.Empty, result2.Value.Id);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    [Fact]
    public void Create_MaximumDecimalPrice_CalculatesTotalCorrectly()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)).Value;
        Money pricePerNight = Money.Create(decimal.MaxValue).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Equal(decimal.MaxValue, result.Value.TotalAmount.Amount);
    }

    [Fact]
    public void Create_MinimumPositiveDecimalPrice_CalculatesTotalCorrectly()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(0.01m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.02m, result.Value.TotalAmount.Amount);
    }

    [Fact]
    public void Create_SingleNightStay_CalculatesTotalCorrectly()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(1);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(99.99m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value.TotalAmount.Amount);
    }

    [Fact]
    public void Create_ExtendedStay_CalculatesTotalCorrectly()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(30);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(200m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Equal(30, dateRange.Nights);
        Assert.Equal(6000m, result.Value.TotalAmount.Amount);
    }

    [Fact]
    public void Create_ZeroPricePerNight_CreateBookingWithZeroTotal()
    {
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(5)).Value;
        Money pricePerNight = Money.Create(0m).Value;

        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.TotalAmount.Amount);
    }
}
