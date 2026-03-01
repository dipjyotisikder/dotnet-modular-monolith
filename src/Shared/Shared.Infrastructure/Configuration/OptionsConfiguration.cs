using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Configuration.Options;
using Shared.Infrastructure.Configuration.Options.Validation;

namespace Shared.Infrastructure.Configuration;

public static class OptionsConfiguration
{
    public static IServiceCollection AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<CorsOptionsValidator>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddOptions<OAuthOptions>()
            .Bind(configuration.GetSection(OAuthOptions.SectionName))
            .ValidateFluentValidation()
            .ValidateOnStart();

        return services;
    }
}
