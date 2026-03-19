using Bookings.Domain.Entities;
using Bookings.Domain.ValueObjects;
using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Seeding;

namespace Bookings.Infrastructure.Seeding;

internal class HotelSeeder(BookingsDbContext dbContext, BookingsUnitOfWork unitOfWork) : Seeder
{
    public override int Priority => 1;

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedData = new List<(string Name, string Description, int Stars, string Street, string City, string Country, string ZipCode)>
        {
            ("Grand Plaza Hotel", "Luxury 5-star hotel with stunning bay views and premium amenities", 5, "123 Marina Boulevard", "San Francisco", "United States", "94105"),
            ("Metropolitan Central", "4-star hotel in the heart of the city with excellent service", 4, "456 Park Avenue", "New York", "United States", "10022"),
            ("Sunset Dream Resort", "3-star resort with beautiful oceanfront location", 3, "789 Sunset Boulevard", "Los Angeles", "United States", "90028")
        };

        foreach (var data in seedData)
        {
            var existingHotel = await dbContext.Hotels.FirstOrDefaultAsync(h => h.Name == data.Name, cancellationToken);

            if (existingHotel == null)
            {
                var hotel = Hotel.Create(data.Name, data.Description, data.Stars,
                    Address.Create(data.Street, data.City, data.Country, data.ZipCode).Value).Value;
                await dbContext.Hotels.AddAsync(hotel, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
