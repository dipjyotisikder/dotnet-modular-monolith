namespace Shared.Infrastructure.Configuration;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Configuration.Options.Validation;
using System.Reflection;

public static class ValidatorExtensions
{
    public static IServiceCollection AddOptionsValidatorsAsSingleton(
        this IServiceCollection services,
        Assembly assembly)
    {
        var validatorType = typeof(IValidator<>);

        var validators = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => typeof(IOptionsValidator).IsAssignableFrom(t))
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType))
            .ToList();

        foreach (var validator in validators)
        {
            var validatorInterfaces = validator.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType)
                .ToList();

            foreach (var validatorInterface in validatorInterfaces)
            {
                services.AddSingleton(validatorInterface, validator);
            }
        }

        return services;
    }

    public static IServiceCollection AddOptionsValidatorsAsStingletonFromAssemblyContaining<T>(
        this IServiceCollection services)
        where T : class, IOptionsValidator
    {
        return AddOptionsValidatorsAsSingleton(services, typeof(T).Assembly);
    }
}
