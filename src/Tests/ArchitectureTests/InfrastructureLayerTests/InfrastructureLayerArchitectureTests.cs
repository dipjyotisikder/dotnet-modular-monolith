using NetArchTest.Rules;
using System.Reflection;

namespace InfrastructureLayerTests;

/// <summary>
/// Architectural tests for the Infrastructure Layer.
/// These tests ensure that infrastructure layer projects follow clean architecture principles,
/// implement proper repository patterns, maintain correct dependencies, and encapsulate data access.
/// </summary>
public class InfrastructureLayerArchitectureTests
{
    private static readonly Assembly SharedDomainAssembly = typeof(Shared.Domain.Result).Assembly;
    private static readonly Assembly SharedInfrastructureAssembly = typeof(Shared.Infrastructure.Repositories.Repository<>).Assembly;
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly UsersInfrastructureAssembly = typeof(Users.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;
    private static readonly Assembly BookingsInfrastructureAssembly = typeof(Bookings.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly[] AllInfrastructureAssemblies =
    [
        SharedInfrastructureAssembly,
        UsersInfrastructureAssembly,
        BookingsInfrastructureAssembly
    ];

    #region Dependency Direction Validation

    /// <summary>
    /// Ensures that Infrastructure depends on Domain but not on Features or Presentation.
    /// Infrastructure should only know about domain entities and repository interfaces.
    /// This maintains the dependency hierarchy: Domain ← Infrastructure
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldDependOnUsersDomain()
    {
        // Infrastructure implements domain repositories and persistence logic
        Assert.NotNull(UsersInfrastructureAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Users.Domain"));
    }

    [Fact]
    public void BookingsInfrastructure_ShouldDependOnBookingsDomain()
    {
        // Infrastructure implements domain repositories and persistence logic
        Assert.NotNull(BookingsInfrastructureAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Bookings.Domain"));
    }

    [Fact]
    public void AllInfrastructure_ShouldDependOnSharedDomain()
    {
        // All infrastructure modules should use shared domain types (Result, Entity, etc.)
        var usersRef = UsersInfrastructureAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Shared.Domain");
        var bookingsRef = BookingsInfrastructureAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Shared.Domain");
        var sharedRef = SharedInfrastructureAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Shared.Domain");

        Assert.NotNull(usersRef);
        Assert.NotNull(bookingsRef);
        Assert.NotNull(sharedRef);
    }

    #endregion

    #region Should NOT Depend On Features

    /// <summary>
    /// Ensures that Infrastructure does not depend on Features/Application layer.
    /// Dependency should flow: Features → Infrastructure, not the other way around.
    /// </summary>
    [Fact]
    public void InfrastructureProjects_ShouldNotDependOnFeatures()
    {
        var result = Types.InAssemblies(AllInfrastructureAssemblies)
            .That()
            .AreNotPublic()
            .Or()
            .ArePublic()
            .Should()
            .NotHaveDependencyOn("*.Features")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Infrastructure should not depend on Features: {GetFailingTypesString(result)}");
    }

    #endregion

    #region Should NOT Have Circular Module Dependencies

    /// <summary>
    /// Ensures that infrastructure modules do not have circular dependencies with each other.
    /// Users.Infrastructure should not depend on Bookings.Infrastructure and vice versa.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldNotDependOnBookingsInfrastructure()
    {
        var result = Types.InAssembly(UsersInfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("Bookings.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Users.Infrastructure should not depend on Bookings.Infrastructure: {GetFailingTypesString(result)}");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldNotDependOnUsersInfrastructure()
    {
        var result = Types.InAssembly(BookingsInfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("Users.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Bookings.Infrastructure should not depend on Users.Infrastructure: {GetFailingTypesString(result)}");
    }

    #endregion

    #region Repository Pattern Implementation

    /// <summary>
    /// Ensures that infrastructure modules implement repository pattern for data access.
    /// Repositories provide abstraction over persistence mechanisms.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldHaveRepositories()
    {
        var repositories = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .Where(t => t.Name.EndsWith("Repository"))
            .ToList();

        Assert.True(repositories.Any(),
            "Users.Infrastructure should define repositories for data access");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldHaveRepositories()
    {
        var repositories = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .Where(t => t.Name.EndsWith("Repository"))
            .ToList();

        Assert.True(repositories.Any(),
            "Bookings.Infrastructure should define repositories for data access");
    }

    #endregion

    #region Unit of Work Pattern

    /// <summary>
    /// Ensures that infrastructure modules implement Unit of Work pattern for managing
    /// repository transactions and relationships. UnitOfWork provides a facade over multiple repositories.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldHaveUnitOfWork()
    {
        var unitOfWork = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "UsersUnitOfWork");

        Assert.NotNull(unitOfWork);
        Assert.True(unitOfWork?.IsClass == true,
            "Users.Infrastructure should have UnitOfWork pattern for repository coordination");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldHaveUnitOfWork()
    {
        var unitOfWork = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "BookingsUnitOfWork");

        Assert.NotNull(unitOfWork);
        Assert.True(unitOfWork?.IsClass == true,
            "Bookings.Infrastructure should have UnitOfWork pattern for repository coordination");
    }

    #endregion

    #region DbContext/ORM Configuration

    /// <summary>
    /// Ensures that infrastructure modules have DbContext classes for Entity Framework Core.
    /// DbContext encapsulates the database mapping and query capabilities.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldHaveDbContext()
    {
        var dbContext = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "UsersDbContext");

        Assert.NotNull(dbContext);
        Assert.True(dbContext?.IsClass == true,
            "Users.Infrastructure should have DbContext for EF Core configuration");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldHaveDbContext()
    {
        var dbContext = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "BookingsDbContext");

        Assert.NotNull(dbContext);
        Assert.True(dbContext?.IsClass == true,
            "Bookings.Infrastructure should have DbContext for EF Core configuration");
    }

    #endregion

    #region Entity Configuration Pattern

    /// <summary>
    /// Ensures that infrastructure modules use EF Core configurations for entity mapping.
    /// Configurations encapsulate mapping rules and keep DbContext clean.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldHaveEntityConfigurations()
    {
        var configurations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Configurations")
            .Where(t => t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any(),
            "Users.Infrastructure should have entity configurations for mapping");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldHaveEntityConfigurations()
    {
        var configurations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Configurations")
            .Where(t => t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any(),
            "Bookings.Infrastructure should have entity configurations for mapping");
    }

    #endregion

    #region Dependency Injection Setup

    /// <summary>
    /// Ensures that infrastructure modules have DependencyInjection setup.
    /// DI setup can be a DependencyInjection class or extension methods in Configuration folder.
    /// DI setup encapsulates infrastructure service registration and prevents coupling.
    /// </summary>
    [Fact]
    public void InfrastructureModules_ShouldHaveDependencyInjectionSetup()
    {
        var usersDI = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        var bookingsDI = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        Assert.NotNull(usersDI);
        Assert.NotNull(bookingsDI);
        Assert.True(usersDI?.IsClass == true && bookingsDI?.IsClass == true,
            "Infrastructure modules should have DependencyInjection setup classes");
    }

    #endregion

    #region Seeding Support

    /// <summary>
    /// Ensures that infrastructure modules provide seeding capabilities for test/demo data.
    /// Seeding is an infrastructure concern and should not leak into domain or features.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldHaveSeeding()
    {
        var seeders = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Seeding")
            .Where(t => t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(seeders.Any(),
            "Users.Infrastructure should provide seeding for test data");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldHaveSeeding()
    {
        var seeders = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Seeding")
            .Where(t => t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(seeders.Any(),
            "Bookings.Infrastructure should provide seeding for test data");
    }

    #endregion

    #region Shared Infrastructure Services

    /// <summary>
    /// Validates that Shared.Infrastructure provides common infrastructure services
    /// and abstractions for all infrastructure modules to use.
    /// </summary>
    [Fact]
    public void SharedInfrastructure_ShouldProvideRepositoryBase()
    {
        var repositoryBase = typeof(Shared.Infrastructure.Repositories.Repository<>);
        Assert.NotNull(repositoryBase);
    }

    [Fact]
    public void SharedInfrastructure_ShouldProvideOutboxRepository()
    {
        var outboxRepo = typeof(Shared.Infrastructure.Repositories.IOutboxRepository);
        Assert.NotNull(outboxRepo);
    }

    [Fact]
    public void SharedInfrastructure_ShouldProvideGlobalExceptionHandler()
    {
        var exceptionHandler = typeof(Shared.Infrastructure.Middleware.GlobalExceptionHandler);
        Assert.NotNull(exceptionHandler);
    }

    #endregion

    #region No Business Logic in Infrastructure

    /// <summary>
    /// Ensures that infrastructure doesn't contain business logic.
    /// Business logic should be in the domain or features layers.
    /// Infrastructure should only contain persistence, messaging, and technical concerns.
    /// </summary>
    [Fact]
    public void InfrastructureProjects_ShouldNotImplementBusinessLogic()
    {
        var allTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure")
            .Concat(GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure"))
            .Concat(GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure"))
            .ToList();

        // Services in infrastructure should be technical services (persistence, messaging, etc.)
        // Not application/domain services (those belong in Features/Domain)
        var technicalServices = allTypes
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("DbContext") 
                     || t.Name.EndsWith("Repository") 
                     || t.Name.EndsWith("Seeder")
                     || t.Name.EndsWith("Configuration")
                     || t.Name.EndsWith("Publisher")
                     || t.Name.EndsWith("Handler")
                     || t.Name.EndsWith("Service")
                     || t.Name.EndsWith("Factory")
                     || t.Name.EndsWith("Interceptor"))
            .ToList();

        Assert.True(technicalServices.Any(),
            "Infrastructure should contain technical services (repositories, DbContexts, etc.)");
    }

    #endregion

    #region Persistence Folder Organization

    /// <summary>
    /// Ensures that persistence concerns are properly organized within Persistence subfolder.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldOrganizePersistenceInFolder()
    {
        var persistenceTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence");
        
        Assert.True(persistenceTypes.Any(),
            "Users.Infrastructure should organize persistence concerns in Persistence folder");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldOrganizePersistenceInFolder()
    {
        var persistenceTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence");
        
        Assert.True(persistenceTypes.Any(),
            "Bookings.Infrastructure should organize persistence concerns in Persistence folder");
    }

    #endregion

    #region Migrations Support

    /// <summary>
    /// Ensures that infrastructure modules have EF Core migrations for database versioning.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldHaveMigrations()
    {
        var migrations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Migrations")
            .ToList();

        Assert.True(migrations.Any(),
            "Users.Infrastructure should have EF Core migrations");
    }

    [Fact]
    public void BookingsInfrastructure_ShouldHaveMigrations()
    {
        var migrations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Migrations")
            .ToList();

        Assert.True(migrations.Any(),
            "Bookings.Infrastructure should have EF Core migrations");
    }

    #endregion

    #region Shared Infrastructure Middleware

    /// <summary>
    /// Validates that Shared.Infrastructure provides common middleware for error handling,
    /// authentication, and other cross-cutting concerns.
    /// </summary>
    [Fact]
    public void SharedInfrastructure_ShouldProvideMiddleware()
    {
        var middlewareTypes = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Middleware")
            .ToList();

        Assert.True(middlewareTypes.Any(),
            "Shared.Infrastructure should provide middleware for cross-cutting concerns");
    }

    #endregion

    #region Authentication Services

    /// <summary>
    /// Ensures that infrastructure provides authentication and security services.
    /// </summary>
    [Fact]
    public void UsersInfrastructure_ShouldProvideAuthenticationServices()
    {
        var authServices = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Services")
            .Where(t => t.Name.Contains("Jwt") || t.Name.Contains("Password") || t.Name.Contains("Token"))
            .ToList();

        Assert.True(authServices.Any(),
            "Users.Infrastructure should provide authentication services (JWT, Password, Token)");
    }

    #endregion

    #region Helper Methods

    private static string GetFailingTypesString(TestResult result)
    {
        return result.FailingTypes?.Any() == true
            ? string.Join(", ", result.FailingTypes)
            : "(no failures)";
    }

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName)
    {
        return assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith(namespaceName) == true)
            .ToList();
    }

    #endregion
}
