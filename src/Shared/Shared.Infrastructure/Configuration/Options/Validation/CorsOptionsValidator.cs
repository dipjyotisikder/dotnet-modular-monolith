using FluentValidation;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public class CorsOptionsValidator : AbstractValidator<CorsOptions>
{
    public CorsOptionsValidator()
    {
        RuleFor(x => x.AllowedOrigins)
            .NotEmpty()
            .WithMessage(ValidationMessages.Cors.AllowedOriginsRequired)
            .Must(origins => origins != null && origins.Length > 0)
            .WithMessage(ValidationMessages.Cors.AllowedOriginsRequired);

        RuleFor(x => x.PreflightMaxAgeMinutes)
            .InclusiveBetween(1, 1440)
            .WithMessage(ValidationMessages.Cors.PreflightMaxAge);
    }
}
