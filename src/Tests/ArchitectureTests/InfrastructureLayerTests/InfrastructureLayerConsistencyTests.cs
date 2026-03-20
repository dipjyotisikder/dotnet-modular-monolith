using System.Reflection;

namespace InfrastructureLayerTests;

public class InfrastructureLayerConsistencyTests
{
    private static readonly Assembly SharedInfrastructureAssembly = typeof(Shared.Infrastructure.Repositories.Repository<>).Assembly;
    private static readonly Assembly UsersInfrastructureAssembly = typeof(Users.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly BookingsInfrastructureAssembly = typeof(Bookings.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void UsersInfrastructureRepositoriesShouldFollowNamingConvention()
    {
        var repositories = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        var properlyNamed = repositories.Where(r => r.Name.EndsWith("Repository") && !r.Name.StartsWith("IRepository")).ToList();
        Assert.True(properlyNamed.Count == repositories.Count);
    }

    [Fact]
    public void BookingsInfrastructureRepositoriesShouldFollowNamingConvention()
    {
        var repositories = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        var properlyNamed = repositories.Where(r => r.Name.EndsWith("Repository") && !r.Name.StartsWith("IRepository")).ToList();
        Assert.True(properlyNamed.Count == repositories.Count);
    }

    [Fact]
    public void UsersInfrastructureConfigurationsShouldBeOrganized()
    {
        var configurations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Configurations")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any());
    }

    [Fact]
    public void BookingsInfrastructureConfigurationsShouldBeOrganized()
    {
        var configurations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Configurations")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any());
    }

    [Fact]
    public void UsersInfrastructureSeedersShouldBeOrganized()
    {
        var userSeeders = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Seeding")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(userSeeders.Any());
    }

    [Fact]
    public void BookingsInfrastructureSeedersShouldBeOrganized()
    {
        var bookingSeeders = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Seeding")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(bookingSeeders.Any());
    }

    [Fact]
    public void UsersInfrastructureDbContextShouldFollowNamingConvention()
    {
        var dbContexts = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("DbContext"))
            .ToList();

        Assert.True(dbContexts.Any());
        Assert.NotNull(dbContexts.FirstOrDefault(d => d.Name == "UsersDbContext"));
    }

    [Fact]
    public void BookingsInfrastructureDbContextShouldFollowNamingConvention()
    {
        var dbContexts = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("DbContext"))
            .ToList();

        Assert.True(dbContexts.Any());
        Assert.NotNull(dbContexts.FirstOrDefault(d => d.Name == "BookingsDbContext"));
    }

    [Fact]
    public void UsersInfrastructureServicesShouldBeOrganized()
    {
        var services = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Services")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        Assert.True(services.Any());
    }

    [Fact]
    public void UsersInfrastructureCanDefineOptions()
    {
        var options = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Options")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        if (options.Any())
            Assert.True(options.All(o => o.Namespace?.Contains("Options") == true));
    }

    [Fact]
    public void InfrastructureModulesShouldAllHaveDependencyInjection()
    {
        var usersDI = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        var bookingsDI = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        Assert.NotNull(usersDI);
        Assert.NotNull(bookingsDI);
        Assert.True(usersDI!.IsAbstract && usersDI.IsSealed);
        Assert.True(bookingsDI!.IsAbstract && bookingsDI.IsSealed);
    }

    [Fact]
    public void UsersInfrastructureUnitOfWorkShouldFollowNamingConvention()
    {
        var unitOfWorks = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UnitOfWork"))
            .ToList();

        Assert.True(unitOfWorks.Any());
        Assert.NotNull(unitOfWorks.FirstOrDefault(u => u.Name == "UsersUnitOfWork"));
    }

    [Fact]
    public void BookingsInfrastructureUnitOfWorkShouldFollowNamingConvention()
    {
        var unitOfWorks = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UnitOfWork"))
            .ToList();

        Assert.True(unitOfWorks.Any());
        Assert.NotNull(unitOfWorks.FirstOrDefault(u => u.Name == "BookingsUnitOfWork"));
    }

    [Fact]
    public void UsersInfrastructureShouldFollowFolderStructure()
    {
        var persistenceTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence").Any();
        var repositoriesTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories").Any();
        var servicesTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Services").Any();
        var seedingTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Seeding").Any();

        Assert.True(persistenceTypes && repositoriesTypes && servicesTypes && seedingTypes);
    }

    [Fact]
    public void BookingsInfrastructureShouldFollowFolderStructure()
    {
        var persistenceTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence").Any();
        var repositoriesTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories").Any();
        var seedingTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Seeding").Any();

        Assert.True(persistenceTypes && repositoriesTypes && seedingTypes);
    }

    [Fact]
    public void UsersInfrastructureMigrationsShouldBeOrganized()
    {
        var migrations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Migrations")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        Assert.True(migrations.Any());
    }

    [Fact]
    public void BookingsInfrastructureMigrationsShouldBeOrganized()
    {
        var migrations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Migrations")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        Assert.True(migrations.Any());
    }

    [Fact]
    public void SharedInfrastructureShouldProvideConsistentAbstractions()
    {
        var repositoryGenericBase = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "Repository`1");

        var outboxRepository = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "IOutboxRepository");

        Assert.NotNull(repositoryGenericBase);
        Assert.NotNull(outboxRepository);
    }

    [Fact]
    public void SharedInfrastructureShouldProvideDbContextAbstractions()
    {
        var outboxDbContext = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "OutboxDbContext");

        var distributedLocksDbContext = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "DistributedLocksDbContext");

        Assert.NotNull(outboxDbContext);
        Assert.NotNull(distributedLocksDbContext);
    }

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName) =>
        assembly.GetTypes().Where(t => t.Namespace?.StartsWith(namespaceName) == true).ToList();
}
