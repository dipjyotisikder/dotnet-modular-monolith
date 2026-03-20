using Bookings.Domain.Entities;
using Bookings.Domain.Enums;
using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.UnitTests.Entities;

public class RoomTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsSuccessWithCorrectlyInitializedRoom()
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100.50m, "USD").Value;
        var maxOccupancy = 2;
        var description = "A comfortable room";

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy, description);

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

    [Theory]
    [InlineData("  101  ", "101")]
    [InlineData("abc", "ABC")]
    [InlineData("  abc  ", "ABC")]
    [InlineData("Room101", "ROOM101")]
    [InlineData("  Room-101  ", "ROOM-101")]
    public void Create_RoomNumberWithWhitespaceOrLowercase_TrimsAndConvertsToUppercase(string input, string expected)
    {
        var hotelId = Guid.NewGuid();
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, input, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.RoomNumber);
    }

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
        var hotelId = Guid.NewGuid();
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, invalidRoomNumber!, roomType, pricePerNight, maxOccupancy);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Room number cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Create_MaxOccupancyZeroOrNegative_ReturnsFailureWithValidationError(int invalidMaxOccupancy)
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, invalidMaxOccupancy);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Max occupancy must be at least 1", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Create_ValidMaxOccupancy_ReturnsSuccess(int validMaxOccupancy)
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, validMaxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(validMaxOccupancy, result.Value.MaxOccupancy);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_PricePerNightZeroOrNegative_ReturnsFailureWithValidationError(decimal invalidAmount)
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(0m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Price per night must be greater than zero", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100.50)]
    [InlineData(9999.99)]
    public void Create_ValidPricePerNight_ReturnsSuccess(decimal validAmount)
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(validAmount, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(pricePerNight, result.Value.PricePerNight);
        Assert.Equal(validAmount, result.Value.PricePerNight.Amount);
    }

    [Fact]
    public void Create_NullDescription_ReturnsSuccessWithNullDescription()
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy, null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  Description  ", "Description")]
    [InlineData("\tDescription\t", "Description")]
    [InlineData("  Multi Word Description  ", "Multi Word Description")]
    public void Create_DescriptionWithWhitespace_TrimsDescription(string? input, string? expected)
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy, input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Description);
    }

    [Theory]
    [InlineData(RoomType.Standard)]
    [InlineData(RoomType.Deluxe)]
    [InlineData(RoomType.Suite)]
    [InlineData(RoomType.Presidential)]
    public void Create_DifferentRoomTypes_ReturnsSuccessWithCorrectRoomType(RoomType roomType)
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(roomType, result.Value.RoomType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Create_UndefinedRoomTypeValue_ReturnsSuccess(int undefinedValue)
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = (RoomType)undefinedValue;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(roomType, result.Value.RoomType);
    }

    [Fact]
    public void Create_EmptyHotelId_ReturnsSuccess()
    {
        var hotelId = Guid.Empty;
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Equal(Guid.Empty, result.Value.HotelId);
    }

    [Fact]
    public void Create_ValidInputs_AssignsUniqueNonEmptyId()
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result1 = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);
        var result2 = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(Guid.Empty, result1.Value.Id);
        Assert.NotEqual(Guid.Empty, result2.Value.Id);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    [Fact]
    public void Create_ValidInputs_SetsIsAvailableToTrue()
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsAvailable);
    }

    [Fact]
    public void Create_OmittedDescription_SetsDescriptionToNull()
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 2;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
    }

    [Fact]
    public void Create_InvalidRoomNumberAndInvalidMaxOccupancy_FailsOnRoomNumberFirst()
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(100m, "USD").Value;
        var maxOccupancy = 0;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.False(result.IsSuccess);
        Assert.Equal("Room number cannot be empty", result.Error);
    }

    [Fact]
    public void Create_InvalidMaxOccupancyAndInvalidPrice_FailsOnMaxOccupancyFirst()
    {
        var hotelId = Guid.NewGuid();
        var roomNumber = "101";
        var roomType = RoomType.Standard;
        var pricePerNight = Money.Create(0m, "USD").Value;
        var maxOccupancy = 0;

        var result = Room.Create(hotelId, roomNumber, roomType, pricePerNight, maxOccupancy);

        Assert.False(result.IsSuccess);
        Assert.Equal("Max occupancy must be at least 1", result.Error);
    }
}
