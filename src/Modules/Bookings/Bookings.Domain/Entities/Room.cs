using Bookings.Domain.Enums;
using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.Entities;

public class Room : Entity
{
    public Guid HotelId { get; private set; }
    public string RoomNumber { get; private set; } = string.Empty;
    public RoomType RoomType { get; private set; }
    public Money PricePerNight { get; private set; } = null!;
    public int MaxOccupancy { get; private set; }
    public string? Description { get; private set; }
    public bool IsAvailable { get; private set; } = true;

    private Room() { }

    public static Result<Room> Create(
        Guid hotelId,
        string roomNumber,
        RoomType roomType,
        Money pricePerNight,
        int maxOccupancy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            return Result.Failure<Room>("Room number cannot be empty", ErrorCodes.VALIDATION_ERROR);

        if (maxOccupancy <= 0)
            return Result.Failure<Room>("Max occupancy must be at least 1", ErrorCodes.VALIDATION_ERROR);

        if (pricePerNight.Amount <= 0)
            return Result.Failure<Room>("Price per night must be greater than zero", ErrorCodes.VALIDATION_ERROR);

        return Result.Success(new Room
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId,
            RoomNumber = roomNumber.Trim().ToUpperInvariant(),
            RoomType = roomType,
            PricePerNight = pricePerNight,
            MaxOccupancy = maxOccupancy,
            Description = description?.Trim()
        });
    }

    public Result SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        return Result.Success();
    }
}
