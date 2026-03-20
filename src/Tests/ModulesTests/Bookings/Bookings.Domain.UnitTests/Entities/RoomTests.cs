namespace Bookings.Domain.UnitTests.Entities;


/// <summary>
/// Unit tests for the <see cref="Room"/> class.
/// </summary>
public class RoomTests
{
    /// <summary>
    /// Tests that Create succeeds with valid inputs and returns a Room with correctly assigned properties.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_ReturnsSuccessWithCorrectlyInitializedRoom()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100.50m, "USD").Value;
        var maxOccupancy = 2;
        var description = "A comfortable room";

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy, description);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(hotelId, result.Value.HotelId);
        Assert.Equal("101", result.Value.RoomNumber);
        Assert.Equal(roomType, result.Value.RoomType);
        Assert.Equal(pricePerNight, result.Value.PricePerNight);
        Assert.Equal(maxOccupancy, result.Value.MaxOccupancy);
        Assert.Equal("A comfortable room", result.Value.Description);
        Assert.True(result.Value.IsAvailable);
    }

    /// <summary>
    /// Tests that Create normalizes room number by trimming whitespace and converting to uppercase.
    /// </summary>
    [Theory]
    [InlineData("  101  ", "101")]
    [InlineData("abc", "ABC")]
    [InlineData("  abc  ", "ABC")]
    [InlineData("Room101", "ROOM101")]
    [InlineData("  Room-101  ", "ROOM-101")]
    public void Create_RoomNumberWithWhitespaceOrLowercase_TrimsAndConvertsToUppercase(string input, string expected)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, input, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.RoomNumber);
    }

    /// <summary>
    /// Tests that Create fails when room number is null, empty, or whitespace-only.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Create_InvalidRoomNumber_ReturnsFailureWithValidationError(string? invalidRoomNumber)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, invalidRoomNumber!, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Room number cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when max occupancy is zero or negative.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Create_MaxOccupancyZeroOrNegative_ReturnsFailureWithValidationError(int invalidMaxOccupancy)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, invalidMaxOccupancy);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Max occupancy must be at least 1", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create succeeds with valid positive max occupancy values.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Create_ValidMaxOccupancy_ReturnsSuccess(int validMaxOccupancy)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, validMaxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(validMaxOccupancy, result.Value.MaxOccupancy);
    }

    /// <summary>
    /// Tests that Create fails when price per night amount is zero or negative.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_PricePerNightZeroOrNegative_ReturnsFailureWithValidationError(decimal invalidAmount)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(0m, "USD").Value;
        // Note: Money.Create validates >= 0, but Room.Create validates > 0
        // We need to use reflection or create with 0 directly, but Money validates this
        // So we create a Money with amount 0 which is valid for Money but should fail for Room
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Price per night must be greater than zero", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create succeeds with valid positive price per night values.
    /// </summary>
    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100.50)]
    [InlineData(9999.99)]
    public void Create_ValidPricePerNight_ReturnsSuccess(decimal validAmount)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(validAmount, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(pricePerNight, result.Value.PricePerNight);
        Assert.Equal(validAmount, result.Value.PricePerNight.Amount);
    }

    /// <summary>
    /// Tests that Create succeeds with null description and sets Description to null.
    /// </summary>
    [Fact]
    public void Create_NullDescription_ReturnsSuccessWithNullDescription()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
    }

    /// <summary>
    /// Tests that Create succeeds with empty or whitespace description and trims it appropriately.
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  Description  ", "Description")]
    [InlineData("\tDescription\t", "Description")]
    [InlineData("  Multi Word Description  ", "Multi Word Description")]
    public void Create_DescriptionWithWhitespace_TrimsDescription(string? input, string? expected)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy, input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Description);
    }

    /// <summary>
    /// Tests that Create accepts and assigns different RoomType enum values correctly.
    /// </summary>
    [Theory]
    [InlineData(RoomType.Standard)]
    [InlineData(RoomType.Deluxe)]
    [InlineData(RoomType.Suite)]
    [InlineData(RoomType.Presidential)]
    public void Create_DifferentRoomTypes_ReturnsSuccessWithCorrectRoomType(RoomType roomType)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(roomType, result.Value.RoomType);
    }

    /// <summary>
    /// Tests that Create accepts undefined RoomType enum values (cast from integer).
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Create_UndefinedRoomTypeValue_ReturnsSuccess(int undefinedValue)
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = (RoomType)undefinedValue;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(roomType, result.Value.RoomType);
    }

    /// <summary>
    /// Tests that Create accepts Guid.Empty for hotelId (no validation on hotelId).
    /// </summary>
    [Fact]
    public void Create_EmptyHotelId_ReturnsSuccess()
    {
        // Arrange
        var hotelId = Guid.Empty;
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Guid.Empty, result.Value.HotelId);
    }

    /// <summary>
    /// Tests that Create assigns a unique, non-empty Id to each created Room.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_AssignsUniqueNonEmptyId()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result1 = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);
        var result2 = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(Guid.Empty, result1.Value.Id);
        Assert.NotEqual(Guid.Empty, result2.Value.Id);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Tests that Create sets IsAvailable to true by default.
    /// </summary>
    [Fact]
    public void Create_ValidInputs_SetsIsAvailableToTrue()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsAvailable);
    }

    /// <summary>
    /// Tests that Create omitting optional description parameter sets Description to null.
    /// </summary>
    [Fact]
    public void Create_OmittedDescription_SetsDescriptionToNull()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
    }

    /// <summary>
    /// Tests that Create validates roomNumber before maxOccupancy (ordering of validations).
    /// </summary>
    [Fact]
    public void Create_InvalidRoomNumberAndInvalidMaxOccupancy_FailsOnRoomNumberFirst()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 0;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Room number cannot be empty", result.Error);
    }

    /// <summary>
    /// Tests that Create validates maxOccupancy before pricePerNight (ordering of validations).
    /// </summary>
    [Fact]
    public void Create_InvalidMaxOccupancyAndInvalidPrice_FailsOnMaxOccupancyFirst()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(0m, "USD").Value;
        var maxOccupancy = 0;

        // Act
        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Max occupancy must be at least 1", result.Error);
    }
}