using NetArchTest.Rules;
using System.Reflection;

namespace FeaturesLayerTests;

public class FeaturesLayerArchitectureTests
{
    private static readonly Assembly SharedApplicationAssembly = typeof(Shared.Application.Abstractions.IApplicationService).Assembly;
    private static readonly Assembly SharedDomainAssembly = typeof(Shared.Domain.Result).Assembly;
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly UsersFeaturesAssembly = typeof(Users.Features.UsersModuleEndpoints).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;
    private static readonly Assembly BookingsFeaturesAssembly = typeof(Bookings.Features.BookingsModuleEndpoints).Assembly;

    private static readonly Assembly[] AllFeaturesAssemblies = [UsersFeaturesAssembly, BookingsFeaturesAssembly];

    [Fact]
    public void UsersFeaturesProjectShouldDependOnUsersDomain()
    {
        Assert.NotNull(UsersFeaturesAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Users.Domain"));
    }

    [Fact]
    public void BookingsFeaturesProjectShouldDependOnBookingsDomain()
    {
        Assert.NotNull(BookingsFeaturesAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Bookings.Domain"));
    }

    [Fact]
    public void FeaturesProjectsShouldUseSharedDomainResultType()
    {
        var result = Types.InAssemblies(AllFeaturesAssemblies)
            .That()
            .HaveName("DependencyInjection")
            .GetTypes();

        Assert.True(result.Any());
    }

    [Fact]
    public void FeaturesProjectsShouldUseSharedApplicationAbstractions()
    {
        var allTypes = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Concat(GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features"))
            .ToList();

        var commandHandlers = allTypes.Where(t => t.Name.EndsWith("CommandHandler")).ToList();
        var queryHandlers = allTypes.Where(t => t.Name.EndsWith("QueryHandler")).ToList();

        Assert.True(commandHandlers.Any() && queryHandlers.Any());
    }

    [Fact]
    public void FeaturesProjectsShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssemblies(AllFeaturesAssemblies)
            .That()
            .AreNotPublic()
            .Or()
            .ArePublic()
            .Should()
            .NotHaveDependencyOn("*.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Features depends on Infrastructure: {GetFailingTypes(result)}");
    }

    [Fact]
    public void UsersFeaturesProjectShouldNotDependOnBookingsFeatures()
    {
        var result = Types.InAssembly(UsersFeaturesAssembly)
            .Should()
            .NotHaveDependencyOn("Bookings.Features")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void BookingsFeaturesProjectShouldNotDependOnUsersFeatures()
    {
        var result = Types.InAssembly(BookingsFeaturesAssembly)
            .Should()
            .NotHaveDependencyOn("Users.Features")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void UsersFeaturesProjectShouldHaveCommandHandlers()
    {
        var commandHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        Assert.True(commandHandlers.Any());
    }

    [Fact]
    public void UsersFeaturesProjectShouldHaveQueryHandlers()
    {
        var queryHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("QueryHandler"))
            .ToList();

        Assert.True(queryHandlers.Any());
    }

    [Fact]
    public void BookingsFeaturesProjectShouldHaveCommandHandlers()
    {
        var commandHandlers = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        Assert.True(commandHandlers.Any());
    }

    [Fact]
    public void BookingsFeaturesProjectShouldHaveQueryHandlers()
    {
        var queryHandlers = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("QueryHandler"))
            .ToList();

        Assert.True(queryHandlers.Any());
    }

    [Fact]
    public void FeaturesModulesShouldHaveValidators()
    {
        var usersValidators = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("CommandValidator") || t.Name.EndsWith("QueryValidator"))
            .ToList();

        var bookingsValidators = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("CommandValidator") || t.Name.EndsWith("QueryValidator"))
            .ToList();

        Assert.True(usersValidators.Any() && bookingsValidators.Any());
    }

    [Fact]
    public void FeaturesModulesShouldHaveEndpoints()
    {
        var usersEndpoints = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("Endpoint"))
            .ToList();

        var bookingsEndpoints = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("Endpoint"))
            .ToList();

        Assert.True(usersEndpoints.Any() && bookingsEndpoints.Any());
    }

    [Fact]
    public void FeaturesModulesShouldHaveDependencyInjectionSetup()
    {
        var usersDI = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        var bookingsDI = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        Assert.NotNull(usersDI);
        Assert.NotNull(bookingsDI);
        Assert.True(usersDI.IsClass && bookingsDI.IsClass);
    }

    [Fact]
    public void FeaturesModulesShouldHaveModuleEndpointsRegistration()
    {
        var usersEndpoints = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .FirstOrDefault(t => t.Name == "UsersModuleEndpoints");

        var bookingsEndpoints = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .FirstOrDefault(t => t.Name == "BookingsModuleEndpoints");

        Assert.NotNull(usersEndpoints);
        Assert.NotNull(bookingsEndpoints);
        Assert.True(usersEndpoints?.IsClass == true && bookingsEndpoints?.IsClass == true);
    }

    [Fact]
    public void SharedApplicationShouldProvideCommandBase()
    {
        var commandBase = typeof(Shared.Application.Abstractions.Command);
        var commandGenericBase = typeof(Shared.Application.Abstractions.Command<>);
        Assert.NotNull(commandBase);
        Assert.NotNull(commandGenericBase);
    }

    [Fact]
    public void SharedApplicationShouldProvideQueryBase()
    {
        var queryBase = typeof(Shared.Application.Abstractions.Query<>);
        Assert.NotNull(queryBase);
    }

    [Fact]
    public void SharedApplicationShouldProvideDomainResultTypeUsage()
    {
        var resultType = typeof(Shared.Domain.Result);
        var resultGenericType = typeof(Shared.Domain.Result<>);
        Assert.NotNull(resultType);
        Assert.NotNull(resultGenericType);
    }

    [Fact]
    public void FeaturesProjectsShouldNotDependOnDatabaseLibraries()
    {
        var result = Types.InAssemblies(AllFeaturesAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Features depends on EF Core: {GetFailingTypes(result)}");
    }

    private static string GetFailingTypes(TestResult result) =>
        result.FailingTypes?.Any() == true ? string.Join(", ", result.FailingTypes) : "(no failures)";

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName) =>
        assembly.GetTypes().Where(t => t.Namespace?.StartsWith(namespaceName) == true).ToList();
}
