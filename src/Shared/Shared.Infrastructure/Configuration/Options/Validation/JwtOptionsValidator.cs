using FluentValidation;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public class JwtOptionsValidator : AbstractValidator<JwtOptions>, IOptionsValidator
{
    public JwtOptionsValidator()
    {
        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .WithMessage(ValidationMessages.Jwt.SecretKeyRequired)
            .MinimumLength(32)
            .WithMessage(ValidationMessages.Jwt.SecretKeyMinLength);

        RuleFor(x => x.Issuer)
            .NotEmpty()
            .WithMessage(ValidationMessages.Jwt.IssuerRequired);

        RuleFor(x => x.Audience)
            .NotEmpty()
            .WithMessage(ValidationMessages.Jwt.AudienceRequired);

        RuleFor(x => x.AccessTokenExpirationHours)
            .InclusiveBetween(1, 24)
            .WithMessage(ValidationMessages.Jwt.AccessTokenExpirationHours);

        RuleFor(x => x.RefreshTokenExpirationDays)
            .InclusiveBetween(1, 90)
            .WithMessage(ValidationMessages.Jwt.RefreshTokenExpirationDays);
    }
}
