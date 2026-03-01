using FluentValidation;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public class FluentValidateOptions<T> : IValidateOptions<T> where T : class
{
    private readonly IValidator<T> _validator;

    public FluentValidateOptions(IValidator<T> validator)
    {
        _validator = validator;
    }

    public ValidateOptionsResult Validate(string? name, T options)
    {
        var result = _validator.Validate(options);

        if (result.IsValid)
            return ValidateOptionsResult.Success;

        var errors = result.Errors.Select(e =>
            $"{e.PropertyName}: {e.ErrorMessage}");

        return ValidateOptionsResult.Fail(errors);
    }
}
