using NetArchTest.Rules;
using System.Reflection;

namespace DomainLayerTests;

/// <summary>
/// Architectural tests for the Domain Layer.
/// These tests ensure that domain projects follow clean architecture principles and best practices.
/// </summary>
public class DomainLayerArchitectureTests
{
    private static readonly Assembly SharedDomainAssembly = typeof(Shared.Domain.Result).Assembly;
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;

    private static readonly Assembly[] AllDomainAssemblies =
    [
        SharedDomainAssembly,
        UsersDomainAssembly,
        BookingsDomainAssembly
    ];

    #region No Infrastructure Dependencies

    /// <summary>
    /// Ensures that Domain layers do not depend on any Infrastructure assemblies.
    /// Domain should be completely isolated from database access, ORM, or persistence concerns.
    /// This is critical for maintaining clean architecture and domain model independence.
    /// </summary>
    [Fact]
    public void DomainProjects_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("*.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, 
            $"Domain layer has dependencies on Infrastructure layer: {GetFailingTypesString(result)}");
    }

    #endregion

    #region No Presentation/Features Dependencies

    /// <summary>
    /// Ensures that Domain layers do not depend on Features/Application layers.
    /// Domain should not know about use cases, commands, queries, or presentation logic.
    /// This enforces unidirectional dependency flow (Features depends on Domain, not vice versa).
    /// </summary>
    [Fact]
    public void DomainProjects_ShouldNotDependOnFeatures()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("*.Features")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer has dependencies on Features layer: {GetFailingTypesString(result)}");
    }

    #endregion

    #region No AppHost Dependencies

    /// <summary>
    /// Ensures that Domain layers do not depend on AppHost.
    /// Domain should be reusable across different entry points and should not know about hosting concerns.
    /// </summary>
    [Fact]
    public void DomainProjects_ShouldNotDependOnAppHost()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("AppHost")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer has dependencies on AppHost: {GetFailingTypesString(result)}");
    }

    #endregion

    #region No Entity Framework References

    /// <summary>
    /// Ensures that Domain layers do not reference Entity Framework Core.
    /// Database mapping concerns (DbSet, DbContext) belong in Infrastructure, not Domain.
    /// This guarantees that domain entities are true domain models, not persistence-aware.
    /// </summary>
    [Fact]
    public void DomainProjects_ShouldNotReferenceSqlServer()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.Data.SqlClient")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer references SQL Server: {GetFailingTypesString(result)}");
    }

    [Fact]
    public void DomainProjects_ShouldNotReferenceEntityFrameworkCore()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer references EF Core: {GetFailingTypesString(result)}");
    }

    #endregion

    #region No Circular Module Dependencies

    /// <summary>
    /// Ensures that module domains do not have circular dependencies with each other.
    /// Users.Domain should not depend on Bookings.Domain and vice versa.
    /// Only Shared.Domain should be shared across modules.
    /// </summary>
    [Fact]
    public void UsersDomain_ShouldNotDependOnBookingsDomain()
    {
        var result = Types.InAssembly(UsersDomainAssembly)
            .Should()
            .NotHaveDependencyOn("Bookings.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Users.Domain should not depend on Bookings.Domain: {GetFailingTypesString(result)}");
    }

    [Fact]
    public void BookingsDomain_ShouldNotDependOnUsersDomain()
    {
        var result = Types.InAssembly(BookingsDomainAssembly)
            .Should()
            .NotHaveDependencyOn("Users.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Bookings.Domain should not depend on Users.Domain: {GetFailingTypesString(result)}");
    }

    #endregion

    #region No HTTP/Web Framework Dependencies

    /// <summary>
    /// Ensures that Domain layers do not depend on System.Web or HTTP-related concerns.
    /// Domain should be framework-agnostic and portable.
    /// </summary>
    [Fact]
    public void DomainProjects_ShouldNotHaveHttpDependencies()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("System.Web")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on System.Web: {GetFailingTypesString(result)}");
    }

    [Fact]
    public void DomainProjects_ShouldNotDependOnAspNetCore()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on AspNetCore: {GetFailingTypesString(result)}");
    }

    #endregion

    #region No Logging in Domain

    /// <summary>
    /// Ensures that Domain layers do not directly use logging frameworks.
    /// Logging is a cross-cutting concern that should be handled in Infrastructure or Application layers.
    /// Domain should raise events; logging happens elsewhere based on those events.
    /// </summary>
    [Fact]
    public void DomainProjects_ShouldNotDependOnSerilog()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Serilog")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on Serilog: {GetFailingTypesString(result)}");
    }

    [Fact]
    public void DomainProjects_ShouldNotDependOnMicrosoftLogging()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.Extensions.Logging")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on Microsoft.Extensions.Logging: {GetFailingTypesString(result)}");
    }

    #endregion

    #region Domain Services Should Be Interfaces

    /// <summary>
    /// Ensures that domain services are defined as interfaces.
    /// Domain services should define contracts, not implementations.
    /// Implementations belong in Infrastructure or Application layers.
    /// </summary>
    [Fact]
    public void DomainServices_ShouldBeInterfaces()
    {
        var userServices = Types.InAssembly(UsersDomainAssembly)
            .That()
            .HaveName("I*")
            .GetTypes();

        var bookingServices = Types.InAssembly(BookingsDomainAssembly)
            .That()
            .HaveName("I*")
            .GetTypes();

        var services = userServices.Concat(bookingServices).ToList();
        var nonInterfaces = services.Where(s => !s.IsInterface).ToList();

        Assert.True(!nonInterfaces.Any(),
            $"Domain services must be interfaces: {string.Join(", ", nonInterfaces.Select(s => s.Name))}");
    }

    #endregion

    #region Repository Interfaces Exist

    /// <summary>
    /// Ensures that repository interfaces are defined in the Domain layer.
    /// Domain aggregates should define the repository contract they need (Inversion of Control).
    /// Implementations are in the Infrastructure layer.
    /// </summary>
    [Fact]
    public void RepositoryInterfaces_ShouldBeDefined()
    {
        var userRepositories = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Repositories");
        var bookingRepositories = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Repositories");

        var allRepositories = userRepositories.Concat(bookingRepositories).ToList();

        // Verify repositories exist (either named I*Repository or found in Repositories namespace)
        Assert.True(allRepositories.Any() || 
                    Types.InAssembly(UsersDomainAssembly).That().HaveName("I*Repository").GetTypes().Any() ||
                    Types.InAssembly(BookingsDomainAssembly).That().HaveName("I*Repository").GetTypes().Any(),
                    "Domain should define repository interfaces");
    }

    #endregion

    #region Result Type Usage

    /// <summary>
    /// Verifies that the Result&lt;T&gt; type exists and is accessible in Domain.
    /// This is used for consistent error handling across domain entities.
    /// </summary>
    [Fact]
    public void Domain_ShouldProvideResultType()
    {
        var resultType = typeof(Shared.Domain.Result);
        var resultGenericType = typeof(Shared.Domain.Result<>);

        Assert.NotNull(resultType);
        Assert.NotNull(resultGenericType);
    }

    #endregion

    #region Entity Base Class

    /// <summary>
    /// Verifies that Entity base class exists in Shared.Domain.
    /// All domain entities should inherit from this base class for consistency.
    /// </summary>
    [Fact]
    public void SharedDomain_ShouldProvideEntityBaseClass()
    {
        var entityType = typeof(Shared.Domain.Entity);
        Assert.NotNull(entityType);
        Assert.True(entityType.IsClass, "Entity should be a class");
    }

    #endregion

    #region Domain Events

    /// <summary>
    /// Verifies that domain event infrastructure exists.
    /// Domain should support raising and handling domain events.
    /// </summary>
    [Fact]
    public void SharedDomain_ShouldProvideDomainEventInterface()
    {
        var domainEventInterface = typeof(Shared.Domain.IDomainEvent);
        Assert.NotNull(domainEventInterface);
        Assert.True(domainEventInterface.IsInterface, "IDomainEvent should be an interface");
    }

    #endregion

    #region Validation Support

    /// <summary>
    /// Verifies that domain validation infrastructure exists.
    /// Domain should support business rule validation.
    /// </summary>
    [Fact]
    public void SharedDomain_ShouldProvideBusinessRuleInterface()
    {
        var businessRuleInterface = typeof(Shared.Domain.Validation.IBusinessRule);
        Assert.NotNull(businessRuleInterface);
        Assert.True(businessRuleInterface.IsInterface, "IBusinessRule should be an interface");
    }

    #endregion

    #region Aggregate Entities Exist

    /// <summary>
    /// Ensures that aggregate root entities are properly defined in module domains.
    /// These should represent the main domain concepts.
    /// </summary>
    [Fact]
    public void UsersDomain_ShouldHaveUserAggregateEntity()
    {
        var userEntity = Types.InAssembly(UsersDomainAssembly)
            .That()
            .HaveName("User")
            .GetTypes();

        Assert.True(userEntity.Any(), "Users.Domain should contain User aggregate entity");
    }

    [Fact]
    public void BookingsDomain_ShouldHaveHotelAndBookingAggregates()
    {
        var hotelEntity = Types.InAssembly(BookingsDomainAssembly)
            .That()
            .HaveName("Hotel")
            .GetTypes();

        var bookingEntity = Types.InAssembly(BookingsDomainAssembly)
            .That()
            .HaveName("Booking")
            .GetTypes();

        Assert.True(hotelEntity.Any() && bookingEntity.Any(), 
            "Bookings.Domain should contain Hotel and Booking aggregate entities");
    }

    #endregion

    #region Value Objects Exist

    /// <summary>
    /// Ensures that value objects are defined to encapsulate domain concepts.
    /// Value objects represent concepts that don't have identity but have important meaning.
    /// </summary>
    [Fact]
    public void UsersDomain_ShouldDefineValueObjects()
    {
        var valueObjects = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.ValueObjects");
        Assert.True(valueObjects.Any(), 
            "Users.Domain should define value objects for domain concepts");
    }

    [Fact]
    public void BookingsDomain_ShouldDefineValueObjects()
    {
        var valueObjects = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.ValueObjects");
        Assert.True(valueObjects.Any(), 
            "Bookings.Domain should define value objects for domain concepts");
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
