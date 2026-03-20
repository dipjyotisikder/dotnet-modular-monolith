using Moq;

namespace Bookings.Domain.UnitTests.Entities;


public class BookingTests
{
    /// <summary>
    /// Tests that BelongsTo returns true when the provided userId matches the booking's GuestId.
    /// </summary>
    [Fact]
    public void BelongsTo_WhenUserIdMatchesGuestId_ReturnsTrue()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)).Value;
        var money = Money.Create(100m, "USD").Value;

        var bookingResult = Booking.Create(guestId, hotelId, roomId, dateRange, money);
        var booking = bookingResult.Value;

        // Act
        var result = booking.BelongsTo(guestId);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that BelongsTo returns false when the provided userId does not match the booking's GuestId.
    /// </summary>
    [Fact]
    public void BelongsTo_WhenUserIdDoesNotMatchGuestId_ReturnsFalse()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)).Value;
        var money = Money.Create(100m, "USD").Value;

        var bookingResult = Booking.Create(guestId, hotelId, roomId, dateRange, money);
        var booking = bookingResult.Value;

        // Act
        var result = booking.BelongsTo(differentUserId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that BelongsTo correctly handles comparison with multiple different user IDs.
    /// This parameterized test verifies the method works consistently across various GUID values.
    /// </summary>
    /// <param name="shouldMatch">Whether the test userId should match the guest ID.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BelongsTo_WithVariousUserIds_ReturnsExpectedResult(bool shouldMatch)
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var userId = shouldMatch ? guestId : Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRangeMock = Mock.Of<DateRange>();
        var moneyMock = Mock.Of<Money>();

        var bookingResult = Booking.Create(guestId, hotelId, roomId, dateRangeMock, moneyMock);
        var booking = bookingResult.Value;

        // Act
        var result = booking.BelongsTo(userId);

        // Assert
        Assert.Equal(shouldMatch, result);
    }

    /// <summary>
    /// Tests that Complete successfully changes status to Completed when the booking is in Confirmed status.
    /// </summary>
    [Fact]
    public void Complete_WhenStatusIsConfirmed_ShouldSucceedAndChangeStatusToCompleted()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;
        booking.ClearDomainEvents();

        // Act
        var result = booking.Complete();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.Single(booking.DomainEvents);
        var domainEvent = booking.DomainEvents.First() as BookingCompletedDomainEvent;
        Assert.NotNull(domainEvent);
        Assert.Equal(booking.Id, domainEvent.BookingId);
        Assert.Equal(guestId, domainEvent.GuestId);
        Assert.Equal(hotelId, domainEvent.HotelId);
    }

    /// <summary>
    /// Tests that Complete returns failure when the booking status is not Confirmed.
    /// </summary>
    /// <param name="statusToSet">The booking status to set before calling Complete.</param>
    /// <param name="setupAction">The name of the setup action to prepare the booking.</param>
    [Theory]
    [InlineData(BookingStatus.Cancelled, "Cancel")]
    [InlineData(BookingStatus.Completed, "Complete")]
    public void Complete_WhenStatusIsNotConfirmed_ShouldReturnFailure(BookingStatus statusToSet, string setupAction)
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;

        // Change status based on setup action
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

        // Act
        var result = booking.Complete();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Only confirmed bookings can be completed", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
        Assert.Equal(statusToSet, booking.Status);
        Assert.Equal(eventCountBeforeAct, booking.DomainEvents.Count);
    }

    /// <summary>
    /// Tests that Complete returns failure when called on an already completed booking.
    /// </summary>
    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFailureAndNotAddDuplicateEvent()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;
        booking.Complete();
        booking.ClearDomainEvents();

        // Act
        var result = booking.Complete();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Only confirmed bookings can be completed", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.Empty(booking.DomainEvents);
    }

    /// <summary>
    /// Tests that Complete does not throw exceptions when status is an undefined enum value.
    /// </summary>
    [Fact]
    public void Complete_WhenStatusIsUndefinedEnumValue_ShouldReturnFailure()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var dateRange = DateRange.Create(DateTime.UtcNow.Date.AddDays(10), DateTime.UtcNow.Date.AddDays(15)).Value;
        var pricePerNight = Money.Create(100m).Value;
        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;

        // Use reflection to set an invalid enum value
        var statusProperty = typeof(Booking).GetProperty("Status");
        statusProperty?.SetValue(booking, (BookingStatus)999);

        // Act
        var result = booking.Complete();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Only confirmed bookings can be completed", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Cancel returns failure when the booking is already cancelled.
    /// </summary>
    [Fact]
    public void Cancel_WhenBookingAlreadyCancelled_ReturnsFailure()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        var firstCancelResult = booking.Cancel("First cancellation");

        // Act
        var result = booking.Cancel("Second cancellation attempt");

        // Assert
        Assert.True(firstCancelResult.IsSuccess);
        Assert.False(result.IsSuccess);
        Assert.Equal("Booking is already cancelled", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Cancel returns failure when the booking status is Completed.
    /// </summary>
    [Fact]
    public void Cancel_WhenBookingCompleted_ReturnsFailure()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: -5);
        booking.Complete();

        // Act
        var result = booking.Cancel("Trying to cancel completed booking");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Completed bookings cannot be cancelled", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Cancel returns failure when check-in date is today.
    /// </summary>
    [Fact]
    public void Cancel_WhenCheckInDateIsToday_ReturnsFailure()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 0);

        // Act
        var result = booking.Cancel("Trying to cancel on check-in day");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot cancel a booking on or after the check-in date", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Cancel returns failure when check-in date is in the past.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void Cancel_WhenCheckInDateIsInPast_ReturnsFailure(int daysInPast)
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: daysInPast);

        // Act
        var result = booking.Cancel("Trying to cancel past booking");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot cancel a booking on or after the check-in date", result.Error);
        Assert.Equal(ErrorCodes.BUSINESS_RULE_VIOLATION, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Cancel successfully cancels a confirmed booking with future check-in date.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(30)]
    [InlineData(365)]
    public void Cancel_WithValidFutureCheckInDate_ReturnsSuccess(int daysFromNow)
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: daysFromNow);
        var beforeCancel = DateTime.UtcNow;

        // Act
        var result = booking.Cancel("Custom cancellation reason");

        // Assert
        var afterCancel = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.CancelledAt);
        Assert.True(booking.CancelledAt >= beforeCancel && booking.CancelledAt <= afterCancel);
        Assert.Equal("Custom cancellation reason", booking.CancellationReason);
        Assert.Single(booking.DomainEvents);
        Assert.IsType<BookingCancelledDomainEvent>(booking.DomainEvents[0]);
    }

    /// <summary>
    /// Tests that Cancel uses the default reason when no reason is provided.
    /// </summary>
    [Fact]
    public void Cancel_WithoutReason_UsesDefaultReason()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 5);

        // Act
        var result = booking.Cancel();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Cancelled by guest", booking.CancellationReason);
    }

    /// <summary>
    /// Tests that Cancel accepts various string formats for the reason parameter.
    /// </summary>
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
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 5);

        // Act
        var result = booking.Cancel(reason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(reason, booking.CancellationReason);
    }

    /// <summary>
    /// Tests that Cancel accepts very long cancellation reasons.
    /// </summary>
    [Fact]
    public void Cancel_WithVeryLongReason_AcceptsReason()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        var longReason = new string('A', 10000);

        // Act
        var result = booking.Cancel(longReason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(longReason, booking.CancellationReason);
    }

    /// <summary>
    /// Tests that Cancel adds the correct domain event with all required properties.
    /// </summary>
    [Fact]
    public void Cancel_WhenSuccessful_AddsDomainEventWithCorrectProperties()
    {
        // Arrange
        var guestId = Guid.NewGuid();
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var booking = CreateValidBooking(guestId, hotelId, roomId, checkInDaysFromNow: 5);
        var reason = "Test cancellation reason";

        // Act
        var result = booking.Cancel(reason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(booking.DomainEvents);
        var domainEvent = Assert.IsType<BookingCancelledDomainEvent>(booking.DomainEvents[0]);
        Assert.Equal(booking.Id, domainEvent.BookingId);
        Assert.Equal(guestId, domainEvent.GuestId);
        Assert.Equal(hotelId, domainEvent.HotelId);
        Assert.Equal(roomId, domainEvent.RoomId);
        Assert.Equal(reason, domainEvent.Reason);
    }

    /// <summary>
    /// Tests that Cancel properly sets the CancelledAt timestamp.
    /// </summary>
    [Fact]
    public void Cancel_WhenSuccessful_SetsCancelledAtToCurrentUtcTime()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        var beforeCancel = DateTime.UtcNow;

        // Act
        var result = booking.Cancel();

        // Assert
        var afterCancel = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.NotNull(booking.CancelledAt);
        Assert.True(booking.CancelledAt.Value >= beforeCancel);
        Assert.True(booking.CancelledAt.Value <= afterCancel);
    }

    /// <summary>
    /// Tests that initial booking state has null CancelledAt and CancellationReason.
    /// </summary>
    [Fact]
    public void Cancel_BeforeCancellation_HasNullCancelledAtAndReason()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 5);

        // Assert
        Assert.Null(booking.CancelledAt);
        Assert.Null(booking.CancellationReason);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    /// <summary>
    /// Tests that Cancel changes the status from Confirmed to Cancelled.
    /// </summary>
    [Fact]
    public void Cancel_WhenSuccessful_ChangesStatusFromConfirmedToCancelled()
    {
        // Arrange
        var booking = CreateValidBooking(checkInDaysFromNow: 5);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);

        // Act
        var result = booking.Cancel();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    /// <summary>
    /// Helper method to create a valid booking with specified check-in date offset.
    /// </summary>
    private Booking CreateValidBooking(int checkInDaysFromNow, int nights = 3)
    {
        return CreateValidBooking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), checkInDaysFromNow, nights);
    }

    /// <summary>
    /// Helper method to create a valid booking with all parameters.
    /// </summary>
    private Booking CreateValidBooking(Guid guestId, Guid hotelId, Guid roomId, int checkInDaysFromNow, int nights = 3)
    {
        var checkIn = DateTime.UtcNow.Date.AddDays(checkInDaysFromNow);
        var checkOut = checkIn.AddDays(nights);
        var dateRange = DateRange.Create(checkIn, checkOut).Value;
        var pricePerNight = Money.Create(100m, "USD").Value;

        var booking = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight).Value;
        booking.ClearDomainEvents(); // Clear creation event for cleaner test assertions

        return booking;
    }

    /// <summary>
    /// Tests that Create fails when guestId is empty.
    /// Expected: Returns failure result with validation error message.
    /// </summary>
    [Fact]
    public void Create_EmptyGuestId_ReturnsFailureResult()
    {
        // Arrange
        Guid guestId = Guid.Empty;
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Guest ID cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when hotelId is empty.
    /// Expected: Returns failure result with validation error message.
    /// </summary>
    [Fact]
    public void Create_EmptyHotelId_ReturnsFailureResult()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.Empty;
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Hotel ID cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when roomId is empty.
    /// Expected: Returns failure result with validation error message.
    /// </summary>
    [Fact]
    public void Create_EmptyRoomId_ReturnsFailureResult()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.Empty;
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Room ID cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create succeeds with valid inputs.
    /// Expected: Returns success result with correctly initialized Booking entity.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_ReturnsSuccessResultWithBooking()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(3);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(150m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
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

    /// <summary>
    /// Tests that Create calculates total amount correctly by multiplying price per night by number of nights.
    /// Expected: TotalAmount equals pricePerNight * nights.
    /// </summary>
    [Theory]
    [InlineData(100, 1, 100)]
    [InlineData(100, 2, 200)]
    [InlineData(100, 5, 500)]
    [InlineData(50.50, 3, 151.50)]
    [InlineData(0, 5, 0)]
    [InlineData(1, 1, 1)]
    public void Create_VariousPricesAndNights_CalculatesTotalAmountCorrectly(decimal priceAmount, int nights, decimal expectedTotal)
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(nights);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(priceAmount).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTotal, result.Value.TotalAmount.Amount);
    }

    /// <summary>
    /// Tests that Create sets CreatedAt to current UTC time.
    /// Expected: CreatedAt is set to a time very close to DateTime.UtcNow.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_SetsCreatedAtToCurrentUtcTime()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;
        DateTime beforeCreate = DateTime.UtcNow;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);
        DateTime afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.CreatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    /// <summary>
    /// Tests that Create initializes CancelledAt as null.
    /// Expected: CancelledAt property is null for a newly created booking.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_InitializesCancelledAtAsNull()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.CancelledAt);
    }

    /// <summary>
    /// Tests that Create initializes CancellationReason as null.
    /// Expected: CancellationReason property is null for a newly created booking.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_InitializesCancellationReasonAsNull()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.CancellationReason);
    }

    /// <summary>
    /// Tests that Create generates a unique non-empty GUID for the booking Id.
    /// Expected: Id is not Guid.Empty and different bookings have different Ids.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_GeneratesUniqueBookingId()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(100m).Value;

        // Act
        Result<Booking> result1 = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);
        Result<Booking> result2 = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(Guid.Empty, result1.Value.Id);
        Assert.NotEqual(Guid.Empty, result2.Value.Id);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Tests that Create with maximum decimal price calculates total correctly.
    /// Expected: Handles large decimal values without overflow.
    /// </summary>
    [Fact]
    public void Create_MaximumDecimalPrice_CalculatesTotalCorrectly()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)).Value;
        Money pricePerNight = Money.Create(decimal.MaxValue).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(decimal.MaxValue, result.Value.TotalAmount.Amount);
    }

    /// <summary>
    /// Tests that Create with minimum positive decimal price works correctly.
    /// Expected: Handles very small decimal values correctly.
    /// </summary>
    [Fact]
    public void Create_MinimumPositiveDecimalPrice_CalculatesTotalCorrectly()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)).Value;
        Money pricePerNight = Money.Create(0.01m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0.02m, result.Value.TotalAmount.Amount);
    }

    /// <summary>
    /// Tests that Create with single night stay calculates total correctly.
    /// Expected: TotalAmount equals pricePerNight for one night.
    /// </summary>
    [Fact]
    public void Create_SingleNightStay_CalculatesTotalCorrectly()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(1);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(99.99m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value.TotalAmount.Amount);
    }

    /// <summary>
    /// Tests that Create with extended stay (many nights) calculates total correctly.
    /// Expected: TotalAmount equals pricePerNight multiplied by number of nights.
    /// </summary>
    [Fact]
    public void Create_ExtendedStay_CalculatesTotalCorrectly()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateTime checkIn = DateTime.UtcNow.Date;
        DateTime checkOut = checkIn.AddDays(30);
        DateRange dateRange = DateRange.Create(checkIn, checkOut).Value;
        Money pricePerNight = Money.Create(200m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(30, dateRange.Nights);
        Assert.Equal(6000m, result.Value.TotalAmount.Amount);
    }

    /// <summary>
    /// Tests that Create with zero price per night creates booking with zero total.
    /// Expected: TotalAmount is zero when price per night is zero.
    /// </summary>
    [Fact]
    public void Create_ZeroPricePerNight_CreateBookingWithZeroTotal()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        Guid hotelId = Guid.NewGuid();
        Guid roomId = Guid.NewGuid();
        DateRange dateRange = DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddDays(5)).Value;
        Money pricePerNight = Money.Create(0m).Value;

        // Act
        Result<Booking> result = Booking.Create(guestId, hotelId, roomId, dateRange, pricePerNight);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.TotalAmount.Amount);
    }
}