using Shared.Domain.Seeding;

namespace Shared.Infrastructure.Seeding;

public abstract class Seeder : ISeeder
{
    public string Name => GetType().Name;
    public virtual int Priority => 0;

    public abstract Task SeedAsync(CancellationToken cancellationToken = default);
}
