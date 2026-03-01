using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<T> ValidateFluentValidation<T>(this OptionsBuilder<T> builder)
        where T : class
    {
        builder.Services.AddSingleton<IValidateOptions<T>>(sp =>
        {
            var validator = sp.GetRequiredService<IValidator<T>>();
            return new FluentValidateOptions<T>(validator);
        });

        return builder;
    }
}
