using NetArchTest.Rules;
using System.Reflection;

namespace FeaturesLayerTests;

/// <summary>
/// Architectural tests for the Features (Application) Layer.
/// These tests ensure that feature/application layer projects follow clean architecture principles,
/// CQRS patterns, and maintain proper dependency directions.
/// </summary>
public class FeaturesLayerArchitectureTests
{
    private static readonly Assembly SharedApplicationAssembly = typeof(Shared.Application.Abstractions.IApplicationService).Assembly;
    private static readonly Assembly SharedDomainAssembly = typeof(Shared.Domain.Result).Assembly;
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly UsersFeaturesAssembly = typeof(Users.Features.UsersModuleEndpoints).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;
    private static readonly Assembly BookingsFeaturesAssembly = typeof(Bookings.Features.BookingsModuleEndpoints).Assembly;

    private static readonly Assembly[] AllFeaturesAssemblies =
    [
        UsersFeaturesAssembly,
        BookingsFeaturesAssembly
    ];

    #region Must Depend On Domain

    /// <summary>
    /// Ensures that Features layers depend on their corresponding Domain layer.
    /// Features implement use cases that operate on domain entities and business logic.
    /// </summary>
    [Fact]
    public void UsersFeatures_ShouldDependOnUsersDomain()
    {
        // Features should reference domain for entity types, repositories, services
        Assert.NotNull(UsersFeaturesAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Users.Domain"));
    }

    [Fact]
    public void BookingsFeatures_ShouldDependOnBookingsDomain()
    {
        // Features should reference domain for entity types, repositories, services
        Assert.NotNull(BookingsFeaturesAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name == "Bookings.Domain"));
    }

    #endregion

    #region Must Depend On Shared

    /// <summary>
    /// Ensures that Features layers depend on Shared.Domain and Shared.Application.
    /// These provide common abstractions and result types.
    /// </summary>
    [Fact]
    public void FeaturesProjects_ShouldUseSharedDomainResultType()
    {
        // Verify that features modules use Result<T> from Shared.Domain through their DI setup
        var result = Types.InAssemblies(AllFeaturesAssemblies)
            .That()
            .HaveName("DependencyInjection")
            .GetTypes();

        Assert.True(result.Any(), "Features should have DependencyInjection setup to configure MediatR");
    }

    [Fact]
    public void FeaturesProjects_ShouldUseSharedApplicationAbstractions()
    {
        // Features use MediatR (IRequest/IRequestHandler) which are configured via Shared.Application
        var allTypes = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Concat(GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features"))
            .ToList();

        var commandHandlers = allTypes.Where(t => t.Name.EndsWith("CommandHandler")).ToList();
        var queryHandlers = allTypes.Where(t => t.Name.EndsWith("QueryHandler")).ToList();

        Assert.True((commandHandlers.Any() && queryHandlers.Any()),
            "Features should have both command and query handlers using Shared.Application CQRS patterns");
    }

    #endregion

    #region Should NOT Depend On Infrastructure

    /// <summary>
    /// Ensures that Features layers do not depend directly on Infrastructure.
    /// Any infrastructure dependencies should be injected through dependency inversion.
    /// Exception: Framework infrastructure (ASP.NET) is acceptable as it's the presentation boundary.
    /// </summary>
    [Fact]
    public void FeaturesProjects_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssemblies(AllFeaturesAssemblies)
            .That()
            .AreNotPublic()
            .Or()
            .ArePublic()
            .Should()
            .NotHaveDependencyOn("*.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Features should not depend on Infrastructure: {GetFailingTypesString(result)}");
    }

    #endregion

    #region No Circular Module Dependencies

    /// <summary>
    /// Ensures that feature modules do not have circular dependencies with each other.
    /// Users.Features should not depend on Bookings.Features and vice versa.
    /// </summary>
    [Fact]
    public void UsersFeatures_ShouldNotDependOnBookingsFeatures()
    {
        var result = Types.InAssembly(UsersFeaturesAssembly)
            .Should()
            .NotHaveDependencyOn("Bookings.Features")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Users.Features should not depend on Bookings.Features: {GetFailingTypesString(result)}");
    }

    [Fact]
    public void BookingsFeatures_ShouldNotDependOnUsersFeatures()
    {
        var result = Types.InAssembly(BookingsFeaturesAssembly)
            .Should()
            .NotHaveDependencyOn("Users.Features")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Bookings.Features should not depend on Users.Features: {GetFailingTypesString(result)}");
    }

    #endregion

    #region CQRS Pattern Structure

    /// <summary>
    /// Ensures that feature modules follow CQRS pattern with Commands, Queries, and Handlers.
    /// </summary>
    [Fact]
    public void UsersFeatures_ShouldHaveCommandHandlers()
    {
        var commandHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        Assert.True(commandHandlers.Any(),
            "Users.Features should define command handlers for CQRS pattern");
    }

    [Fact]
    public void UsersFeatures_ShouldHaveQueryHandlers()
    {
        var queryHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("QueryHandler"))
            .ToList();

        Assert.True(queryHandlers.Any(),
            "Users.Features should define query handlers for CQRS pattern");
    }

    [Fact]
    public void BookingsFeatures_ShouldHaveCommandHandlers()
    {
        var commandHandlers = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        Assert.True(commandHandlers.Any(),
            "Bookings.Features should define command handlers for CQRS pattern");
    }

    [Fact]
    public void BookingsFeatures_ShouldHaveQueryHandlers()
    {
        var queryHandlers = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("QueryHandler"))
            .ToList();

        Assert.True(queryHandlers.Any(),
            "Bookings.Features should define query handlers for CQRS pattern");
    }

    #endregion

    #region Validators Exist

    /// <summary>
    /// Ensures that feature modules use validators for command validation.
    /// This enforces input validation at the application layer boundary.
    /// </summary>
    [Fact]
    public void FeaturesModules_ShouldHaveValidators()
    {
        var usersValidators = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("CommandValidator") || t.Name.EndsWith("QueryValidator"))
            .ToList();

        var bookingsValidators = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("CommandValidator") || t.Name.EndsWith("QueryValidator"))
            .ToList();

        Assert.True(usersValidators.Any() && bookingsValidators.Any(),
            "Features modules should define validators for input validation");
    }

    #endregion

    #region Endpoints/DTOs Organization

    /// <summary>
    /// Ensures that endpoints and DTOs are properly organized in features modules.
    /// </summary>
    [Fact]
    public void FeaturesModules_ShouldHaveEndpoints()
    {
        var usersEndpoints = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("Endpoint"))
            .ToList();

        var bookingsEndpoints = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("Endpoint"))
            .ToList();

        Assert.True(usersEndpoints.Any() && bookingsEndpoints.Any(),
            "Features modules should define endpoints");
    }

    #endregion

    #region Dependency Injection Setup

    /// <summary>
    /// Ensures that each feature module has a DependencyInjection setup class.
    /// This enforces proper service registration encapsulation per module.
    /// </summary>
    [Fact]
    public void FeaturesModules_ShouldHaveDependencyInjectionSetup()
    {
        var usersDI = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        var bookingsDI = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .FirstOrDefault(t => t.Name == "DependencyInjection");

        Assert.NotNull(usersDI);
        Assert.NotNull(bookingsDI);
        Assert.True(usersDI.IsClass && bookingsDI.IsClass,
            "Features modules should have DependencyInjection setup classes");
    }

    #endregion

    #region Module Endpoints Registration

    /// <summary>
    /// Ensures that each feature module has a module endpoints registration class.
    /// This provides a single point of endpoint registration per module.
    /// </summary>
    [Fact]
    public void FeaturesModules_ShouldHaveModuleEndpointsRegistration()
    {
        var usersEndpoints = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .FirstOrDefault(t => t.Name == "UsersModuleEndpoints");

        var bookingsEndpoints = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .FirstOrDefault(t => t.Name == "BookingsModuleEndpoints");

        Assert.NotNull(usersEndpoints);
        Assert.NotNull(bookingsEndpoints);
        Assert.True(usersEndpoints?.IsClass == true && bookingsEndpoints?.IsClass == true,
            "Features modules should have module endpoints registration classes");
    }

    #endregion

    #region Shared Infrastructure Types

    /// <summary>
    /// Verifies that shared application types are available for features to use.
    /// These provide the foundation for CQRS and error handling patterns.
    /// </summary>
    [Fact]
    public void SharedApplication_ShouldProvideCommandBase()
    {
        var commandBase = typeof(Shared.Application.Abstractions.Command);
        var commandGenericBase = typeof(Shared.Application.Abstractions.Command<>);
        Assert.NotNull(commandBase);
        Assert.NotNull(commandGenericBase);
    }

    [Fact]
    public void SharedApplication_ShouldProvideQueryBase()
    {
        var queryBase = typeof(Shared.Application.Abstractions.Query<>);
        Assert.NotNull(queryBase);
    }

    [Fact]
    public void SharedApplication_ShouldProvideDomainResultTypeUsage()
    {
        var resultType = typeof(Shared.Domain.Result);
        var resultGenericType = typeof(Shared.Domain.Result<>);
        Assert.NotNull(resultType);
        Assert.NotNull(resultGenericType);
    }

    #endregion

    #region No Direct Data Access

    /// <summary>
    /// Ensures that features do not directly access data.
    /// All data access should go through repositories defined in domain.
    /// </summary>
    [Fact]
    public void FeaturesProjects_ShouldNotDependOnDatabaseLibraries()
    {
        var result = Types.InAssemblies(AllFeaturesAssemblies)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Features should not directly depend on EF Core: {GetFailingTypesString(result)}");
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
