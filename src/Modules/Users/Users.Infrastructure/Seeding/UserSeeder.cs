using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Seeding;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories;

namespace Users.Infrastructure.Seeding;

internal class UserSeeder(UsersDbContext dbContext, UsersUnitOfWork unitOfWork) : Seeder
{
    public override int Priority => 0;

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedData = new List<(string Email, string Name, string Tier, bool IsAdmin)>
        {
            ("admin@monolithic.dev", "Admin User", "premium", true),
            ("user1@monolithic.dev", "John Doe", "standard", false),
            ("user2@monolithic.dev", "Jane Smith", "standard", false),
            ("user3@monolithic.dev", "Bob Johnson", "standard", false)
        };

        foreach (var (email, name, tier, isAdmin) in seedData)
        {
            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (existingUser == null)
            {
                var user = User.Create(email, name, passwordHash: "hashed_password", tier: tier).Value;
                if (isAdmin)
                    user.AddRole("Admin");
                await dbContext.Users.AddAsync(user, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
