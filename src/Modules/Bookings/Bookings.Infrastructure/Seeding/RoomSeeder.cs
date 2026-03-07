using Bookings.Domain.Entities;
using Bookings.Domain.Enums;
using Bookings.Domain.ValueObjects;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Seeding;

namespace Bookings.Infrastructure.Seeding;

internal class RoomSeeder : Seeder
{
    private readonly BookingsDbContext _dbContext;

    public override int Priority => 2;

    public RoomSeeder(BookingsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var hotelRoomData = new[] {
            ("Grand Plaza Hotel", new[] {
                ("101", RoomType.Standard, 150m, 2, "Standard room with city view"),
                ("201", RoomType.Deluxe, 250m, 2, "Deluxe room with panoramic bay views"),
                ("301", RoomType.Suite, 450m, 4, "Spacious suite with living room and bay windows"),
                ("401", RoomType.Presidential, 800m, 6, "Premium presidential suite with full amenities")
            }),
            ("Metropolitan Central", new[] {
                ("101", RoomType.Standard, 120m, 2, "Cozy standard room"),
                ("201", RoomType.Deluxe, 200m, 2, "Comfortable deluxe room with modern design")
            }),
            ("Sunset Dream Resort", new[] {
                ("101", RoomType.Standard, 100m, 2, "Standard oceanfront room"),
                ("201", RoomType.Suite, 300m, 4, "Suite with ocean view and balcony")
            })
        };

        foreach (var (hotelName, roomData) in hotelRoomData)
        {
            await UpsertRoomsForHotelAsync(hotelName, roomData, cancellationToken);
        }
    }

    private async Task UpsertRoomsForHotelAsync(
        string hotelName,
        (string RoomNumber, RoomType Type, decimal Price, int Capacity, string Description)[] rooms,
    CancellationToken cancellationToken)
    {
        var hotel = await _dbContext.Hotels
            .FirstOrDefaultAsync(h => h.Name == hotelName, cancellationToken);

        if (hotel == null)
            return;

        var existingRooms = await _dbContext.Rooms
            .Where(r => r.HotelId == hotel.Id)
            .Select(r => r.RoomNumber)
            .ToHashSetAsync(cancellationToken);

        var roomsToAdd = new List<Room>();
        foreach (var (roomNumber, type, price, capacity, description) in rooms)
        {
            if (!existingRooms.Contains(roomNumber))
            {
                roomsToAdd.Add(Room.Create(hotel.Id, roomNumber, type, Money.Create(price).Value, capacity, description).Value);
            }
        }

        if (roomsToAdd.Count != 0)
        {
            await _dbContext.Rooms.AddRangeAsync(roomsToAdd, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
