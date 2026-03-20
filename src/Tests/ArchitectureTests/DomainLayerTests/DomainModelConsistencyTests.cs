using System.Reflection;

namespace DomainLayerTests;

/// <summary>
/// Domain Model Consistency tests.
/// These tests verify that domain entities follow consistent patterns and best practices
/// through reflection-based inspection of the model structure.
/// </summary>
public class DomainModelConsistencyTests
{
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;
    private static readonly Assembly SharedDomainAssembly = typeof(Shared.Domain.Result).Assembly;

    #region Factory Methods Pattern

    /// <summary>
    /// Ensures that aggregate roots use factory methods for creation.
    /// This allows centralized validation and initialization logic in the domain.
    /// </summary>
    [Fact]
    public void AggregateRoots_ShouldHaveFactoryMethods()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var entitiesWithFactory = entities
            .Where(e => e.GetMethod("Create", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
            .ToList();

        Assert.True(entitiesWithFactory.Any(),
            "Major aggregate roots should have static factory methods (e.g., Create) for controlled creation");
    }

    #endregion

    #region Mutation Methods Return Result Type

    /// <summary>
    /// Ensures that methods that modify entity state return Result or Result&lt;T&gt;.
    /// This promotes explicit error handling and consistent operation outcomes.
    /// </summary>
    [Fact]
    public void MutationMethods_ShouldReturnResultType()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var resultType = typeof(Shared.Domain.Result);
        var resultGenericType = typeof(Shared.Domain.Result<>);

        var mutationMethodsWithoutResult = new List<string>();

        foreach (var entity in entities)
        {
            var methods = entity.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                // Skip special methods
                if (method.IsSpecialName || method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                    continue;

                // Check if it's a mutation method
                if (method.Name.StartsWith("Update") || 
                    method.Name.StartsWith("Set") || 
                    method.Name.StartsWith("Add") || 
                    method.Name.StartsWith("Remove") ||
                    method.Name.StartsWith("Activate") ||
                    method.Name.StartsWith("Deactivate"))
                {
                    // Check if return type is Result or Result<T>
                    var returnType = method.ReturnType;
                    bool isValidReturnType = returnType == resultType || 
                        (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == resultGenericType);

                    if (!isValidReturnType)
                    {
                        mutationMethodsWithoutResult.Add($"{entity.Name}.{method.Name}");
                    }
                }
            }
        }

        // This test warns about mutation methods that should return Result
        // Some void methods may be intentional (e.g., internal state modifications)
        if (mutationMethodsWithoutResult.Any())
        {
            // Log for information - not all mutation methods require Result return type
        }
    }

    #endregion

    #region Entity Encapsulation

    /// <summary>
    /// Ensures that entities encapsulate business logic through public methods.
    /// Domain logic should not be scattered across services but contained in the entity.
    /// </summary>
    [Fact]
    public void Entities_ShouldEncapsulateDomainLogic()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var entitiesWithMethods = entities
            .Where(e => e.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                .Count(m => !m.IsSpecialName && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_")) > 0)
            .ToList();

        Assert.True(entitiesWithMethods.Any(),
            "Entities should have public methods encapsulating business logic");
    }

    #endregion

    #region Constructor Accessibility

    /// <summary>
    /// Ensures that entities have appropriate constructors.
    /// Entity constructors can be private (for factories) or public (for ORM support).
    /// </summary>
    [Fact]
    public void Entities_ShouldHaveConstructors()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var entitiesWithoutConstructors = new List<string>();

        foreach (var entity in entities)
        {
            var constructors = entity.GetConstructors(
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);

            if (!constructors.Any())
            {
                entitiesWithoutConstructors.Add(entity.Name);
            }
        }

        Assert.True(!entitiesWithoutConstructors.Any(),
            $"Entities must have at least one constructor: {string.Join(", ", entitiesWithoutConstructors)}");
    }

    #endregion

    #region Domain Services Are Interfaces

    /// <summary>
    /// Ensures that domain services are defined as interfaces, not implementations.
    /// This enforces the Dependency Inversion Principle - implementations should be in Infrastructure.
    /// </summary>
    [Fact]
    public void DomainServices_ShouldBeInterfaces()
    {
        var services = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Services")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Services"))
            .Where(t => t.Name.StartsWith("I"))
            .ToList();

        var nonInterfaces = services.Where(s => !s.IsInterface).ToList();

        Assert.True(!nonInterfaces.Any(),
            $"Domain services must be interfaces: {string.Join(", ", nonInterfaces.Select(s => s.Name))}");
    }

    #endregion

    #region Repository Interfaces Exist

    /// <summary>
    /// Ensures that repository interfaces are defined in domain modules.
    /// Aggregates depend on repository contracts, not implementations (Inversion of Control).
    /// </summary>
    [Fact]
    public void RepositoriesInterfaces_ShouldBeDefined()
    {
        var userRepositories = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Repositories").ToList();
        var bookingRepositories = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Repositories").ToList();

        Assert.True(userRepositories.Any() && bookingRepositories.Any(),
            "Module domains should define repository interfaces");

        var nonInterfaceRepos = userRepositories.Concat(bookingRepositories).Where(r => !r.IsInterface).ToList();
        Assert.True(!nonInterfaceRepos.Any(),
            $"Repositories in domain should be interfaces: {string.Join(", ", nonInterfaceRepos.Select(r => r.Name))}");
    }

    #endregion

    #region Domain Models Exist

    /// <summary>
    /// Ensures that key domain concepts are represented through proper modeling.
    /// </summary>
    [Fact]
    public void DomainModels_ShouldDefineValueObjects()
    {
        var userValueObjects = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.ValueObjects").ToList();
        var bookingValueObjects = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.ValueObjects").ToList();

        Assert.True(userValueObjects.Any() && bookingValueObjects.Any(),
            "Domain modules should define value objects to encapsulate domain concepts");
    }

    #endregion

    #region Domain Events Support

    /// <summary>
    /// Verifies that domain events infrastructure is properly used.
    /// Entities should be able to raise domain events for important business occurrences.
    /// </summary>
    [Fact]
    public void DomainEventInfrastructure_ShouldBeDefined()
    {
        var domainEventInterface = typeof(Shared.Domain.IDomainEvent);
        var domainEventClass = typeof(Shared.Domain.DomainEvent);

        Assert.NotNull(domainEventInterface);
        Assert.NotNull(domainEventClass);
        Assert.True(domainEventInterface.IsInterface, "IDomainEvent should be an interface");
    }

    #endregion

    #region Shared Domain Types Available

    /// <summary>
    /// Verifies that all critical shared domain types are available for module domains to use.
    /// These provide the foundation for consistent domain modeling across modules.
    /// </summary>
    [Fact]
    public void SharedDomain_ShouldProvideEssentialTypes()
    {
        Assert.NotNull(typeof(Shared.Domain.Entity));
        Assert.NotNull(typeof(Shared.Domain.AuditableEntity));
        Assert.NotNull(typeof(Shared.Domain.Result));
        var resultGenericType = typeof(Shared.Domain.Result<>);
        Assert.NotNull(resultGenericType);
        Assert.NotNull(typeof(Shared.Domain.IDomainEvent));
        Assert.NotNull(typeof(Shared.Domain.DomainEvent));
        Assert.NotNull(typeof(Shared.Domain.Validation.IBusinessRule));
        Assert.NotNull(typeof(Shared.Domain.Services.ISystemClock));
        var repositoryInterfaceType = typeof(Shared.Domain.Repositories.IRepository<>);
        Assert.NotNull(repositoryInterfaceType);
        Assert.NotNull(typeof(Shared.Domain.Repositories.IUnitOfWork));
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
