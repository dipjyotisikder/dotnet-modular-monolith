namespace Shared.Domain.Seeding;

public interface ISeeder
{
    string Name { get; }
    int Priority { get; }
    Task SeedAsync(CancellationToken cancellationToken = default);
}
