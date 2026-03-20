using NetArchTest.Rules;
using System.Reflection;

namespace DomainLayerTests;

public class DomainLayerArchitectureTests
{
    private static readonly Assembly SharedDomainAssembly = typeof(Shared.Domain.Result).Assembly;
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;

    private static readonly Assembly[] AllDomainAssemblies = [SharedDomainAssembly, UsersDomainAssembly, BookingsDomainAssembly];

    [Fact]
    public void DomainProjectsShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("*.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer depends on Infrastructure: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotDependOnFeatures()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("*.Features")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer depends on Features: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotDependOnAppHost()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("AppHost")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer depends on AppHost: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotReferenceSqlServer()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.Data.SqlClient")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer references SQL Server: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotReferenceEntityFrameworkCore()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer references EF Core: {GetFailingTypes(result)}");
    }

    [Fact]
    public void UsersDomainShouldNotDependOnBookingsDomain()
    {
        var result = Types.InAssembly(UsersDomainAssembly)
            .Should()
            .NotHaveDependencyOn("Bookings.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Users.Domain depends on Bookings.Domain: {GetFailingTypes(result)}");
    }

    [Fact]
    public void BookingsDomainShouldNotDependOnUsersDomain()
    {
        var result = Types.InAssembly(BookingsDomainAssembly)
            .Should()
            .NotHaveDependencyOn("Users.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Bookings.Domain depends on Users.Domain: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotHaveHttpDependencies()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("System.Web")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer depends on System.Web: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotDependOnAspNetCore()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer depends on AspNetCore: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotDependOnSerilog()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Serilog")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer depends on Serilog: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainProjectsShouldNotDependOnMicrosoftLogging()
    {
        var result = Types.InAssemblies(AllDomainAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.Extensions.Logging")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain layer depends on logging: {GetFailingTypes(result)}");
    }

    [Fact]
    public void DomainServicesShouldBeInterfaces()
    {
        var userServices = Types.InAssembly(UsersDomainAssembly).That().HaveName("I*").GetTypes();
        var bookingServices = Types.InAssembly(BookingsDomainAssembly).That().HaveName("I*").GetTypes();
        var services = userServices.Concat(bookingServices).ToList();
        var nonInterfaces = services.Where(s => !s.IsInterface).ToList();

        Assert.True(!nonInterfaces.Any(), $"Domain services must be interfaces: {string.Join(", ", nonInterfaces.Select(s => s.Name))}");
    }

    [Fact]
    public void RepositoryInterfacesShouldBeDefined()
    {
        var userRepositories = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Repositories");
        var bookingRepositories = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Repositories");
        var allRepositories = userRepositories.Concat(bookingRepositories).ToList();

        Assert.True(allRepositories.Any() ||
                    Types.InAssembly(UsersDomainAssembly).That().HaveName("I*Repository").GetTypes().Any() ||
                    Types.InAssembly(BookingsDomainAssembly).That().HaveName("I*Repository").GetTypes().Any(),
                    "Domain should define repository interfaces");
    }

    [Fact]
    public void DomainShouldProvideResultType()
    {
        var resultType = typeof(Shared.Domain.Result);
        var resultGenericType = typeof(Shared.Domain.Result<>);

        Assert.NotNull(resultType);
        Assert.NotNull(resultGenericType);
    }

    [Fact]
    public void SharedDomainShouldProvideEntityBaseClass()
    {
        var entityType = typeof(Shared.Domain.Entity);
        Assert.NotNull(entityType);
        Assert.True(entityType.IsClass);
    }

    [Fact]
    public void SharedDomainShouldProvideDomainEventInterface()
    {
        var domainEventInterface = typeof(Shared.Domain.IDomainEvent);
        Assert.NotNull(domainEventInterface);
        Assert.True(domainEventInterface.IsInterface);
    }

    [Fact]
    public void SharedDomainShouldProvideBusinessRuleInterface()
    {
        var businessRuleInterface = typeof(Shared.Domain.Validation.IBusinessRule);
        Assert.NotNull(businessRuleInterface);
        Assert.True(businessRuleInterface.IsInterface);
    }

    [Fact]
    public void UsersDomainShouldHaveUserAggregateEntity()
    {
        var userEntity = Types.InAssembly(UsersDomainAssembly).That().HaveName("User").GetTypes();
        Assert.True(userEntity.Any());
    }

    [Fact]
    public void BookingsDomainShouldHaveHotelAndBookingAggregates()
    {
        var hotelEntity = Types.InAssembly(BookingsDomainAssembly).That().HaveName("Hotel").GetTypes();
        var bookingEntity = Types.InAssembly(BookingsDomainAssembly).That().HaveName("Booking").GetTypes();

        Assert.True(hotelEntity.Any() && bookingEntity.Any());
    }

    [Fact]
    public void UsersDomainShouldDefineValueObjects()
    {
        var valueObjects = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.ValueObjects");
        Assert.True(valueObjects.Any());
    }

    [Fact]
    public void BookingsDomainShouldDefineValueObjects()
    {
        var valueObjects = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.ValueObjects");
        Assert.True(valueObjects.Any());
    }

    private static string GetFailingTypes(TestResult result) =>
        result.FailingTypes?.Any() == true ? string.Join(", ", result.FailingTypes) : "(no failures)";

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName) =>
        assembly.GetTypes().Where(t => t.Namespace?.StartsWith(namespaceName) == true).ToList();
}
