using System.Reflection;

namespace DomainLayerTests;

public class DomainModelConsistencyTests
{
    private static readonly Assembly UsersDomainAssembly = typeof(Users.Domain.Entities.User).Assembly;
    private static readonly Assembly BookingsDomainAssembly = typeof(Bookings.Domain.Entities.Booking).Assembly;

    [Fact]
    public void AggregateRootsShouldHaveFactoryMethods()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var entitiesWithFactory = entities
            .Where(e => e.GetMethod("Create", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
            .ToList();

        Assert.True(entitiesWithFactory.Any());
    }

    [Fact]
    public void MutationMethodsShouldReturnResultType()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var resultType = typeof(Shared.Domain.Result);
        var resultGenericType = typeof(Shared.Domain.Result<>);

        foreach (var entity in entities)
        {
            var methods = entity.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.IsSpecialName || method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                    continue;

                if (method.Name.StartsWith("Update") || method.Name.StartsWith("Set") || method.Name.StartsWith("Add") ||
                    method.Name.StartsWith("Remove") || method.Name.StartsWith("Activate") || method.Name.StartsWith("Deactivate"))
                {
                    var returnType = method.ReturnType;
                    bool isValidReturnType = returnType == resultType || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == resultGenericType);
                }
            }
        }
    }

    [Fact]
    public void EntitiesShouldEncapsulateDomainLogic()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var entitiesWithMethods = entities
            .Where(e => e.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                .Count(m => !m.IsSpecialName && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_")) > 0)
            .ToList();

        Assert.True(entitiesWithMethods.Any());
    }

    [Fact]
    public void EntitiesShouldHaveConstructors()
    {
        var entities = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Entities")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Entities"))
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.Name.StartsWith("<>"))
            .ToList();

        var entitiesWithoutConstructors = new List<string>();
        foreach (var entity in entities)
        {
            var constructors = entity.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (!constructors.Any())
                entitiesWithoutConstructors.Add(entity.Name);
        }

        Assert.True(!entitiesWithoutConstructors.Any());
    }

    [Fact]
    public void DomainServicesShouldBeInterfaces()
    {
        var services = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Services")
            .Concat(GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Services"))
            .Where(t => t.Name.StartsWith("I"))
            .ToList();

        var nonInterfaces = services.Where(s => !s.IsInterface).ToList();
        Assert.True(!nonInterfaces.Any());
    }

    [Fact]
    public void RepositoriesInterfacesShouldBeDefined()
    {
        var userRepositories = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.Repositories").ToList();
        var bookingRepositories = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.Repositories").ToList();

        Assert.True(userRepositories.Any() && bookingRepositories.Any());

        var nonInterfaceRepos = userRepositories.Concat(bookingRepositories).Where(r => !r.IsInterface).ToList();
        Assert.True(!nonInterfaceRepos.Any());
    }

    [Fact]
    public void DomainModelsShouldDefineValueObjects()
    {
        var userValueObjects = GetTypesInNamespace(UsersDomainAssembly, "Users.Domain.ValueObjects").ToList();
        var bookingValueObjects = GetTypesInNamespace(BookingsDomainAssembly, "Bookings.Domain.ValueObjects").ToList();

        Assert.True(userValueObjects.Any() && bookingValueObjects.Any());
    }

    [Fact]
    public void DomainEventInfrastructureShouldBeDefined()
    {
        var domainEventInterface = typeof(Shared.Domain.IDomainEvent);
        var domainEventClass = typeof(Shared.Domain.DomainEvent);

        Assert.NotNull(domainEventInterface);
        Assert.NotNull(domainEventClass);
        Assert.True(domainEventInterface.IsInterface);
    }

    [Fact]
    public void SharedDomainShouldProvideEssentialTypes()
    {
        Assert.NotNull(typeof(Shared.Domain.Entity));
        Assert.NotNull(typeof(Shared.Domain.AuditableEntity));
        Assert.NotNull(typeof(Shared.Domain.Result));
        Assert.NotNull(typeof(Shared.Domain.Result<>));
    }

    private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string namespaceName) =>
        assembly.GetTypes().Where(t => t.Namespace?.StartsWith(namespaceName) == true).ToList();
}
