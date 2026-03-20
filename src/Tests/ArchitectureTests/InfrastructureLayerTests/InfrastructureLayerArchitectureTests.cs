using NetArchTest.Rules;
using System.Reflection;

namespace InfrastructureLayerTests;

public class InfrastructureLayerArchitectureTests
{
    private static readonly Assembly SharedDomainAssembly = typeof(Shared.Domain.Result).Assembly;
    private static readonly Assembly SharedInfrastructureAssembly = typeof(Shared.Infrastructure.Repositories.Repository<>).Assembly;
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly UsersInfrastructureAssembly = typeof(Users.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;
    private static readonly Assembly BookingsInfrastructureAssembly = typeof(Bookings.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly[] AllInfrastructureAssemblies = [SharedInfrastructureAssembly, UsersInfrastructureAssembly, BookingsInfrastructureAssembly];

    [Fact]
    public void UsersInfrastructureProjectShouldDependOnUsersDomain()
    {
        Assert.NotNull(UsersInfrastructureAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Users.Domain"));
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldDependOnBookingsDomain()
    {
        Assert.NotNull(BookingsInfrastructureAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Bookings.Domain"));
    }

    [Fact]
    public void AllInfrastructureProjectsShouldDependOnSharedDomain()
    {
        var usersRef = UsersInfrastructureAssembly.GetReferencedAssemblies().FirstOrDefault(a => a.Name == "Shared.Domain");
        var bookingsRef = BookingsInfrastructureAssembly.GetReferencedAssemblies().FirstOrDefault(a => a.Name == "Shared.Domain");
        var sharedRef = SharedInfrastructureAssembly.GetReferencedAssemblies().FirstOrDefault(a => a.Name == "Shared.Domain");

        Assert.NotNull(usersRef);
        Assert.NotNull(bookingsRef);
        Assert.NotNull(sharedRef);
    }

    [Fact]
    public void InfrastructureProjectsShouldNotDependOnFeatures()
    {
        var result = Types.InAssemblies(AllInfrastructureAssemblies)
            .That()
            .AreNotPublic()
            .Or()
            .ArePublic()
            .Should()
            .NotHaveDependencyOn("*.Features")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Infrastructure depends on Features: {GetFailingTypes(result)}");
    }

    [Fact]
    public void UsersInfrastructureProjectShouldNotDependOnBookingsInfrastructure()
    {
        var result = Types.InAssembly(UsersInfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("Bookings.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldNotDependOnUsersInfrastructure()
    {
        var result = Types.InAssembly(BookingsInfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("Users.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void UsersInfrastructureProjectShouldHaveRepositories()
    {
        var repositories = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .Where(t => t.Name.EndsWith("Repository"))
            .ToList();

        Assert.True(repositories.Any());
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldHaveRepositories()
    {
        var repositories = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .Where(t => t.Name.EndsWith("Repository"))
            .ToList();

        Assert.True(repositories.Any());
    }

    [Fact]
    public void UsersInfrastructureProjectShouldHaveUnitOfWork()
    {
        var unitOfWork = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "UsersUnitOfWork");

        Assert.NotNull(unitOfWork);
        Assert.True(unitOfWork?.IsClass == true);
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldHaveUnitOfWork()
    {
        var unitOfWork = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Repositories")
            .FirstOrDefault(t => t.Name == "BookingsUnitOfWork");

        Assert.NotNull(unitOfWork);
        Assert.True(unitOfWork?.IsClass == true);
    }

    [Fact]
    public void UsersInfrastructureProjectShouldHaveDbContext()
    {
        var dbContext = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "UsersDbContext");

        Assert.NotNull(dbContext);
        Assert.True(dbContext?.IsClass == true);
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldHaveDbContext()
    {
        var dbContext = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence")
            .FirstOrDefault(t => t.Name == "BookingsDbContext");

        Assert.NotNull(dbContext);
        Assert.True(dbContext?.IsClass == true);
    }

    [Fact]
    public void UsersInfrastructureProjectShouldHaveEntityConfigurations()
    {
        var configurations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Configurations")
            .Where(t => t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any());
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldHaveEntityConfigurations()
    {
        var configurations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Configurations")
            .Where(t => t.Name.EndsWith("Configuration"))
            .ToList();

        Assert.True(configurations.Any());
    }

    [Fact]
    public void InfrastructureModulesShouldHaveDependencyInjectionSetup()
    {
        var usersDI = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        var bookingsDI = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        Assert.NotNull(usersDI);
        Assert.NotNull(bookingsDI);
        Assert.True(usersDI?.IsClass == true && bookingsDI?.IsClass == true);
    }

    [Fact]
    public void UsersInfrastructureProjectShouldHaveSeeding()
    {
        var seeders = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Seeding")
            .Where(t => t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(seeders.Any());
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldHaveSeeding()
    {
        var seeders = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Seeding")
            .Where(t => t.Name.EndsWith("Seeder"))
            .ToList();

        Assert.True(seeders.Any());
    }

    [Fact]
    public void SharedInfrastructureShouldProvideRepositoryBase()
    {
        var repositoryBase = typeof(Shared.Infrastructure.Repositories.Repository<>);
        Assert.NotNull(repositoryBase);
    }

    [Fact]
    public void SharedInfrastructureShouldProvideOutboxRepository()
    {
        var outboxRepo = typeof(Shared.Infrastructure.Repositories.IOutboxRepository);
        Assert.NotNull(outboxRepo);
    }

    [Fact]
    public void SharedInfrastructureShouldProvideGlobalExceptionHandler()
    {
        var exceptionHandler = typeof(Shared.Infrastructure.Middleware.GlobalExceptionHandler);
        Assert.NotNull(exceptionHandler);
    }

    [Fact]
    public void InfrastructureProjectsShouldNotImplementBusinessLogic()
    {
        var allTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure")
            .Concat(GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure"))
            .Concat(GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure"))
            .ToList();

        var technicalServices = allTypes
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("DbContext") || t.Name.EndsWith("Repository") || t.Name.EndsWith("Seeder") ||
                       t.Name.EndsWith("Configuration") || t.Name.EndsWith("Publisher") || t.Name.EndsWith("Handler") ||
                       t.Name.EndsWith("Service") || t.Name.EndsWith("Factory") || t.Name.EndsWith("Interceptor"))
            .ToList();

        Assert.True(technicalServices.Any());
    }

    [Fact]
    public void UsersInfrastructureProjectShouldOrganizePersistenceInFolder()
    {
        var persistenceTypes = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence");
        Assert.True(persistenceTypes.Any());
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldOrganizePersistenceInFolder()
    {
        var persistenceTypes = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence");
        Assert.True(persistenceTypes.Any());
    }

    [Fact]
    public void UsersInfrastructureProjectShouldHaveMigrations()
    {
        var migrations = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Persistence.Migrations").ToList();
        Assert.True(migrations.Any());
    }

    [Fact]
    public void BookingsInfrastructureProjectShouldHaveMigrations()
    {
        var migrations = GetTypesInNamespace(BookingsInfrastructureAssembly, "Bookings.Infrastructure.Persistence.Migrations").ToList();
        Assert.True(migrations.Any());
    }

    [Fact]
    public void SharedInfrastructureShouldProvideMiddleware()
    {
        var middlewareTypes = GetTypesInNamespace(SharedInfrastructureAssembly, "Shared.Infrastructure.Middleware").ToList();
        Assert.True(middlewareTypes.Any());
    }

    [Fact]
    public void UsersInfrastructureProjectShouldProvideAuthenticationServices()
    {
        var authServices = GetTypesInNamespace(UsersInfrastructureAssembly, "Users.Infrastructure.Services")
            .Where(t => t.Name.Contains("Jwt") || t.Name.Contains("Password") || t.Name.Contains("Token"))
            .ToList();

        Assert.True(authServices.Any());
    }

    private static string GetFailingTypes(TestResult result) =>
        result.FailingTypes?.Any() == true ? string.Join(", ", result.FailingTypes) : "(no failures)";

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName) =>
        assembly.GetTypes().Where(t => t.Namespace?.StartsWith(namespaceName) == true).ToList();
}
