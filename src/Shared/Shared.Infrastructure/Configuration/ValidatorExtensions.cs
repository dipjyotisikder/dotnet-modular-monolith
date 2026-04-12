namespace Shared.Infrastructure.Configuration;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Configuration.Options.Validation;
using System.Reflection;

public static class ValidatorExtensions
{
    extension(IServiceCollection services)
    {
        public void AddOptionsValidatorsAsSingletonFromAssemblyContaining<T>()
            where T : class, IOptionsValidator
        {
            services.AddOptionsValidatorsAsSingleton(typeof(T).Assembly);
        }
        
        private void AddOptionsValidatorsAsSingleton(Assembly assembly)
        {
            var validatorType = typeof(IValidator<>);

            var validators = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
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
        }
    }
}
