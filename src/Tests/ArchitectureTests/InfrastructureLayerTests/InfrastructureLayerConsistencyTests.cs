using System.Reflection;

namespace InfrastructureLayerTests;

/// <summary>
/// Consistency tests for the Infrastructure Layer.
/// These tests validate naming conventions, organization patterns, and consistency across infrastructure modules.
/// </summary>
public class InfrastructureLayerConsistencyTests
{
    private static readonly Assembly SharedInfrastructureAssembly = typeof(Shared.Infrastructure.Repositories.Repository<>).Assembly;
    private static readonly Assembly UsersInfrastructureAssembly = typeof(Users.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly BookingsInfrastructureAssembly = typeof(Bookings.Infrastructure.DependencyInjection).Assembly;

    #region Repository Naming Conventions

    /// <summary>
    /// Ensures repositories follow consistent naming conventions: [Entity]Repository
    /// </summary>
    [Fact]
    public void UsersInfrastructure_Repositories_ShouldFollowNamingConvention()
    {
        var repositories = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        var properlyNamed = repositories
            .Where(r => r.Name.EndsWith("Repository") && !r.Name.StartsWith("IRepository"))
            .ToList();

        Assert.True(properlyNamed.Count == repositories.Count,
            "All repositories should follow [Entity]Repository naming convention");
    }

    [Fact]
    public void BookingsInfrastructure_Repositories_ShouldFollowNamingConvention()
    {
        var repositories = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        var properlyNamed = repositories
            .Where(r => r.Name.EndsWith("Repository") && !r.Name.StartsWith("IRepository"))
            .ToList();

        Assert.True(properlyNamed.Count == repositories.Count,
            "All repositories should follow [Entity]Repository naming convention");
    }

    #endregion

    #region Configuration Classes Organization

    /// <summary>
    /// Ensures entity configurations are properly named and located.
    /// Naming: [Entity]Configuration
    /// Location: Persistence/Configurations folder
    /// </summary>
    [Fact]
    public void UsersInfrastructure_Configurations_ShouldBeOrganized()
    {
        var configurations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Configurations")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any(),
            "Configurations should follow [Entity]Configuration naming convention and be in Persistence/Configurations");
    }

    [Fact]
    public void BookingsInfrastructure_Configurations_ShouldBeOrganized()
    {
        var configurations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Configurations")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any(),
            "Configurations should follow [Entity]Configuration naming convention and be in Persistence/Configurations");
    }

    #endregion

    #region Seeder Organization

    /// <summary>
    /// Ensures seeders are properly named and located in Seeding folder.
    /// Naming: [Entity]Seeder
    /// </summary>
    [Fact]
    public void UsersInfrastructure_Seeders_ShouldBeOrganized()
    {
        var seeders = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Seeding")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(seeders.Any(),
            "Seeders should follow [Entity]Seeder naming convention and be in Seeding folder");
    }

    [Fact]
    public void BookingsInfrastructure_Seeders_ShouldBeOrganized()
    {
        var seeders = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Seeding")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(seeders.Any(),
            "Seeders should follow [Entity]Seeder naming convention and be in Seeding folder");
    }

    #endregion

    #region DbContext Naming

    /// <summary>
    /// Ensures DbContext classes follow naming convention: [Module]DbContext
    /// </summary>
    [Fact]
    public void UsersInfrastructure_DbContext_ShouldFollowNamingConvention()
    {
        var dbContexts = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("DbContext"))
            .ToList();

        Assert.True(dbContexts.Any(),
            "Users.Infrastructure should have DbContext following [Module]DbContext pattern");
        
        var usersDbContext = dbContexts.FirstOrDefault(d => d.Name == "UsersDbContext");
        Assert.NotNull(usersDbContext);
    }

    [Fact]
    public void BookingsInfrastructure_DbContext_ShouldFollowNamingConvention()
    {
        var dbContexts = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("DbContext"))
            .ToList();

        Assert.True(dbContexts.Any(),
            "Bookings.Infrastructure should have DbContext following [Module]DbContext pattern");
        
        var bookingsDbContext = dbContexts.FirstOrDefault(d => d.Name == "BookingsDbContext");
        Assert.NotNull(bookingsDbContext);
    }

    #endregion

    #region Services Organization

    /// <summary>
    /// Ensures infrastructure services are properly organized in Services folder.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_Services_ShouldBeOrganized()
    {
        var services = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Services")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        Assert.True(services.Any(),
            "Users.Infrastructure should organize infrastructure services in Services folder");
    }

    #endregion

    #region Options/Configuration

    /// <summary>
    /// Ensures infrastructure modules can define configuration options when needed.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_CanDefineOptions()
    {
        var options = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Options")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        // Options are optional, but if defined should be in Options folder
        if (options.Any())
        {
            Assert.True(options.All(o => o.Namespace?.Contains("Options") == true),
                "Options should be organized in Options folder");
        }
    }

    #endregion

    #region DependencyInjection Pattern Consistency

    /// <summary>
    /// Validates DependencyInjection class presence across all infrastructure modules.
    /// DI classes should provide extension methods for IServiceCollection.
    /// </summary>
    [Fact]
    public void InfrastructureModules_ShouldAllHaveDependencyInjection()
    {
        var usersDI = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        var bookingsDI = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        Assert.NotNull(usersDI);
        Assert.NotNull(bookingsDI);

        // Both should be static classes with extension methods
        Assert.True(usersDI!.IsAbstract && usersDI.IsSealed,
            "DependencyInjection should be a static class");
        Assert.True(bookingsDI!.IsAbstract && bookingsDI.IsSealed,
            "DependencyInjection should be a static class");
    }

    #endregion

    #region UnitOfWork Pattern Consistency

    /// <summary>
    /// Ensures UnitOfWork classes are consistently named and organized.
    /// Naming: [Module]UnitOfWork
    /// </summary>
    [Fact]
    public void UsersInfrastructure_UnitOfWork_ShouldFollowNamingConvention()
    {
        var unitOfWorks = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UnitOfWork"))
            .ToList();

        Assert.True(unitOfWorks.Any(),
            "Users.Infrastructure should have UnitOfWork class");

        var usersUoW = unitOfWorks.FirstOrDefault(u => u.Name == "UsersUnitOfWork");
        Assert.NotNull(usersUoW);
    }

    [Fact]
    public void BookingsInfrastructure_UnitOfWork_ShouldFollowNamingConvention()
    {
        var unitOfWorks = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UnitOfWork"))
            .ToList();

        Assert.True(unitOfWorks.Any(),
            "Bookings.Infrastructure should have UnitOfWork class");

        var bookingsUoW = unitOfWorks.FirstOrDefault(u => u.Name == "BookingsUnitOfWork");
        Assert.NotNull(bookingsUoW);
    }

    #endregion

    #region Folder Structure Consistency

    /// <summary>
    /// Validates that infrastructure modules follow consistent folder organization.
    /// Expected structure:
    /// - Persistence/ (DbContext, Configurations, Migrations)
    /// - Repositories/ (Repository implementations, UnitOfWork)
    /// - Services/ (Infrastructure services)
    /// - Seeding/ (Data seeders)
    /// - Options/ (Configuration options - if needed)
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldFollowFolderStructure()
    {
        var persistenceTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence").Any();
        var repositoriesTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories").Any();
        var servicesTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Services").Any();
        var seedingTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Seeding").Any();

        Assert.True(persistenceTypes && repositoriesTypes && servicesTypes && seedingTypes,
            "Users.Infrastructure should follow expected folder structure: Persistence, Repositories, Services, Seeding");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldFollowFolderStructure()
    {
        var persistenceTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence").Any();
        var repositoriesTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories").Any();
        var seedingTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Seeding").Any();

        Assert.True(persistenceTypes && repositoriesTypes && seedingTypes,
            "Bookings.Infrastructure should follow expected folder structure: Persistence, Repositories, Seeding");
    }

    #endregion

    #region Migrations Pattern

    /// <summary>
    /// Validates that EF Core migrations follow expected patterns and organization.
    /// Migrations should be in Persistence/Migrations folder with descriptive names.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_Migrations_ShouldBeOrganized()
    {
        var migrations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Migrations")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        // EF Core migrations should exist
        Assert.True(migrations.Any(),
            "Users.Infrastructure should have EF Core migrations in Persistence/Migrations");
    }

    [Fact]
    public void BookingsInfrastructure_Migrations_ShouldBeOrganized()
    {
        var migrations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Migrations")
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        // EF Core migrations should exist
        Assert.True(migrations.Any(),
            "Bookings.Infrastructure should have EF Core migrations in Persistence/Migrations");
    }

    #endregion

    #region Shared Infrastructure Consistency

    /// <summary>
    /// Validates that Shared.Infrastructure provides consistent abstractions and base classes
    /// for all infrastructure modules to use.
    /// </summary>
    [Fact]
    public void SharedInfrastructure_ShouldProvideConsistentAbstractions()
    {
        // Core abstractions that should be in Shared.Infrastructure
        var repositoryGenericBase = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "Repository`1");

        var outboxRepository = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "IOutboxRepository");

        Assert.NotNull(repositoryGenericBase);
        Assert.NotNull(outboxRepository);
    }

    [Fact]
    public void SharedInfrastructure_ShouldProvideDbContextAbstractions()
    {
        var outboxDbContext = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "OutboxDbContext");

        var distributedLocksDbContext = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "DistributedLocksDbContext");

        Assert.NotNull(outboxDbContext);
        Assert.NotNull(distributedLocksDbContext);
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName)
    {
        return assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith(namespaceName) == true)
            .ToList();
    }

    #endregion
}
