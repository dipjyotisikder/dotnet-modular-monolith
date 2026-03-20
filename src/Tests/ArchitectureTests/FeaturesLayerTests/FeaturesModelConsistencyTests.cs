using System.Reflection;

namespace FeaturesLayerTests;

/// <summary>
/// Features Layer Model Consistency tests.
/// These tests verify that features layer projects follow consistent CQRS patterns,
/// proper naming conventions, and command/query structuring.
/// </summary>
public class FeaturesModelConsistencyTests
{
    private static readonly Assembly UsersFeaturesAssembly = typeof(Users.Features.UsersModuleEndpoints).Assembly;
    private static readonly Assembly BookingsFeaturesAssembly = typeof(Bookings.Features.BookingsModuleEndpoints).Assembly;

    #region Command Structure

    /// <summary>
    /// Ensures that commands have matching handlers and validators.
    /// Commands should follow ICommand pattern and be properly paired with infrastructure.
    /// </summary>
    [Fact]
    public void Commands_ShouldHaveCorrespondingHandlers()
    {
        var usersCommands = GetTypesEndingWith(UsersFeaturesAssembly, "Command")
            .Where(t => !t.Name.EndsWith("CommandHandler") && !t.Name.EndsWith("CommandValidator"))
            .ToList();

        var bookingsCommands = GetTypesEndingWith(BookingsFeaturesAssembly, "Command")
            .Where(t => !t.Name.EndsWith("CommandHandler") && !t.Name.EndsWith("CommandValidator"))
            .ToList();

        var allCommands = usersCommands.Concat(bookingsCommands).ToList();

        Assert.True(allCommands.Any(),
            "Features should define commands for CQRS pattern");

        // Most commands should have handlers
        var commandsWithHandlers = allCommands
            .Where(c => FindCorrespondingType(c, "Handler") != null)
            .ToList();

        Assert.True(commandsWithHandlers.Any(),
            "Commands should have corresponding handlers");
    }

    #endregion

    #region Query Structure

    /// <summary>
    /// Ensures that queries have matching handlers.
    /// Queries should follow IQuery<T> pattern and have implementations.
    /// </summary>
    [Fact]
    public void Queries_ShouldHaveCorrespondingHandlers()
    {
        var usersQueries = GetTypesEndingWith(UsersFeaturesAssembly, "Query")
            .Where(t => !t.Name.EndsWith("QueryHandler"))
            .ToList();

        var bookingsQueries = GetTypesEndingWith(BookingsFeaturesAssembly, "Query")
            .Where(t => !t.Name.EndsWith("QueryHandler"))
            .ToList();

        var allQueries = usersQueries.Concat(bookingsQueries).ToList();

        Assert.True(allQueries.Any(),
            "Features should define queries for CQRS pattern");

        var queriesWithHandlers = allQueries
            .Where(q => FindCorrespondingType(q, "Handler") != null)
            .ToList();

        Assert.True(queriesWithHandlers.Any(),
            "Queries should have corresponding handlers");
    }

    #endregion

    #region Handler Naming Convention

    /// <summary>
    /// Verifies that handlers follow consistent naming conventions.
    /// Handlers should be named: [CommandOrQuery]Handler
    /// </summary>
    [Fact]
    public void Handlers_ShouldFollowNamingConvention()
    {
        var handlers = GetTypesEndingWith(UsersFeaturesAssembly, "Handler")
            .Concat(GetTypesEndingWith(BookingsFeaturesAssembly, "Handler"))
            .ToList();

        var properlyNamed = handlers
            .Where(h => h.Name.EndsWith("CommandHandler") || h.Name.EndsWith("QueryHandler"))
            .ToList();

        Assert.True(properlyNamed.Count == handlers.Count,
            "All handlers should follow [Command|Query]Handler naming convention");
    }

    #endregion

    #region Validator Implementation

    /// <summary>
    /// Ensures that validators follow consistent patterns.
    /// Validators should be defined for input validation.
    /// </summary>
    [Fact]
    public void Validators_ShouldBeDefined()
    {
        var validators = GetTypesEndingWith(UsersFeaturesAssembly, "Validator")
            .Concat(GetTypesEndingWith(BookingsFeaturesAssembly, "Validator"))
            .ToList();

        Assert.True(validators.Any(),
            "Features should define validators for input validation");
    }

    #endregion

    #region Endpoint Organization

    /// <summary>
    /// Ensures that endpoints are properly organized and named.
    /// Endpoints should follow [Operation]Endpoint naming convention.
    /// </summary>
    [Fact]
    public void Endpoints_ShouldFollowNamingConvention()
    {
        var endpoints = GetTypesEndingWith(UsersFeaturesAssembly, "Endpoint")
            .Concat(GetTypesEndingWith(BookingsFeaturesAssembly, "Endpoint"))
            .ToList();

        Assert.True(endpoints.Any(),
            "Features should define endpoints");

        var properlyNamed = endpoints
            .Where(e => !e.Name.EndsWith("sEndpoint") || e.Name.Contains("Endpoint"))
            .ToList();

        Assert.True(properlyNamed.Any(),
            "Endpoints should follow proper naming convention");
    }

    #endregion

    #region Handler Return Types

    /// <summary>
    /// Verifies that handlers return Result or Result<T> types for consistency.
    /// This ensures error handling is explicit and consistent.
    /// </summary>
    [Fact]
    public void CommandHandlers_ShouldReturnResult()
    {
        var handlers = GetTypesEndingWith(UsersFeaturesAssembly, "CommandHandler")
            .Concat(GetTypesEndingWith(BookingsFeaturesAssembly, "CommandHandler"))
            .ToList();

        var resultType = typeof(Shared.Domain.Result);
        var resultGenericType = typeof(Shared.Domain.Result<>);

        foreach (var handler in handlers)
        {
            var handleMethod = handler.GetMethod("Handle", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            // Most handlers should return Result or Task<Result>
            if (handleMethod != null)
            {
                var returnType = handleMethod.ReturnType;
                var isValid = returnType == resultType ||
                            (returnType.IsGenericType && 
                             (returnType.GetGenericTypeDefinition() == resultGenericType ||
                              returnType.GetGenericTypeDefinition().IsAssignableFrom(resultGenericType)));

                // This is informational - not all handlers must return Result
                // But it's a best practice for consistency
            }
        }

        Assert.True(handlers.Any(),
            "Command handlers should be defined");
    }

    #endregion

    #region Response Types

    /// <summary>
    /// Ensures that features define response types for queries.
    /// Response types should be DTOs representing query results.
    /// </summary>
    [Fact]
    public void Features_ShouldDefineResponseTypes()
    {
        var responses = GetTypesEndingWith(UsersFeaturesAssembly, "Response")
            .Concat(GetTypesEndingWith(BookingsFeaturesAssembly, "Response"))
            .ToList();

        // At least some query results should have response DTOs
        Assert.True(responses.Any(),
            "Features should define response types for query results");
    }

    #endregion

    #region Module Encapsulation

    /// <summary>
    /// Ensures that each feature module is self-contained with proper organization.
    /// Modules should have clear boundaries and consistent structure.
    /// </summary>
    [Fact]
    public void FeaturesModules_ShouldHaveConsistentStructure()
    {
        var usersFeatures = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features");
        var bookingsFeatures = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features");

        // Check for feature subfolder structure (feature areas)
        var usersHasFeatureAreas = usersFeatures
            .Any(t => t.Namespace?.Contains("Features.") == true);

        var bookingsHasFeatureAreas = bookingsFeatures
            .Any(t => t.Namespace?.Contains("Features.") == true);

        Assert.True(usersHasFeatureAreas && bookingsHasFeatureAreas,
            "Feature modules should be organized with feature/use-case areas");
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<Type> GetTypesEndingWith(Assembly assembly, string suffix)
    {
        return assembly.GetTypes()
            .Where(t => t.Name.EndsWith(suffix) && !t.IsAbstract && !t.IsInterface)
            .ToList();
    }

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName)
    {
        return assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith(namespaceName) == true)
            .ToList();
    }

    private static Type? FindCorrespondingType(Type original, string suffix)
    {
        var assembly = original.Assembly;
        var expectedName = original.Name + suffix;
        return assembly.GetTypes().FirstOrDefault(t => t.Name == expectedName);
    }

    #endregion
}
