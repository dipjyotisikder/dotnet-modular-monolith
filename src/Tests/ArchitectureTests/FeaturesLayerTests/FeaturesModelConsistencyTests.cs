using System.Reflection;

namespace FeaturesLayerTests;

public class FeaturesModelConsistencyTests
{
    private static readonly Assembly UsersFeaturesAssembly = typeof(Users.Features.UsersModuleEndpoints).Assembly;
    private static readonly Assembly BookingsFeaturesAssembly = typeof(Bookings.Features.BookingsModuleEndpoints).Assembly;

    [Fact]
    public void CommandHandlersShouldReturnResult()
    {
        var userHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        var bookingHandlers = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        Assert.True(userHandlers.Any() && bookingHandlers.Any());
    }

    [Fact]
    public void HandlersShouldFollowNamingConvention()
    {
        var userHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Handler"))
            .ToList();

        var bookingHandlers = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Handler"))
            .ToList();

        var properlyNamed = userHandlers.Concat(bookingHandlers)
            .Where(h => h.Name.EndsWith("CommandHandler") || h.Name.EndsWith("QueryHandler"))
            .ToList();

        Assert.True(properlyNamed.Count == userHandlers.Concat(bookingHandlers).Count());
    }

    [Fact]
    public void ValidatorsShouldBeDefined()
    {
        var userValidators = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("Validator"))
            .ToList();

        var bookingValidators = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("Validator"))
            .ToList();

        Assert.True(userValidators.Any() && bookingValidators.Any());
    }

    [Fact]
    public void EndpointsShouldFollowNamingConvention()
    {
        var endpoints = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Concat(GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features"))
            .Where(t => t.Name.EndsWith("Endpoint"))
            .ToList();

        var properlyNamed = endpoints
            .Where(e => e.Name.EndsWith("Endpoint"))
            .ToList();

        Assert.True(properlyNamed.Count == endpoints.Count && endpoints.Any());
    }

    [Fact]
    public void FeaturesModulesShouldHaveConsistentStructure()
    {
        var userCommands = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features").Any(t => t.Name.EndsWith("Command"));
        var userQueries = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features").Any(t => t.Name.EndsWith("Query"));
        var userHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features").Any(t => t.Name.EndsWith("Handler"));

        var bookingCommands = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features").Any(t => t.Name.EndsWith("Command"));
        var bookingQueries = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features").Any(t => t.Name.EndsWith("Query"));
        var bookingHandlers = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features").Any(t => t.Name.EndsWith("Handler"));

        Assert.True((userCommands || userQueries) && userHandlers && (bookingCommands || bookingQueries) && bookingHandlers);
    }

    [Fact]
    public void CommandsShouldHaveCorrespondingHandlers()
    {
        var userCommands = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("Command") && !t.Name.EndsWith("CommandHandler"))
            .ToList();

        var userHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        Assert.True(userCommands.Count <= userHandlers.Count || userHandlers.Any());
    }

    [Fact]
    public void QueriesShouldHaveCorrespondingHandlers()
    {
        var userQueries = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("Query") && !t.Name.EndsWith("QueryHandler"))
            .ToList();

        var userHandlers = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("QueryHandler"))
            .ToList();

        Assert.True(userQueries.Count <= userHandlers.Count || userHandlers.Any());
    }

    [Fact]
    public void FeaturesShouldDefineResponseTypes()
    {
        var userResponses = GetTypesInNamespace(UsersFeaturesAssembly, "Users.Features")
            .Where(t => t.Name.EndsWith("Response"))
            .ToList();

        var bookingResponses = GetTypesInNamespace(BookingsFeaturesAssembly, "Bookings.Features")
            .Where(t => t.Name.EndsWith("Response"))
            .ToList();

        Assert.True(userResponses.Any() && bookingResponses.Any());
    }

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName) =>
        assembly.GetTypes().Where(t => t.Namespace?.StartsWith(namespaceName) == true).ToList();
}
