using FluentValidation;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public class OAuthOptionsValidator : AbstractValidator<OAuthOptions>, IOptionsValidator
{
    public OAuthOptionsValidator()
    {
        RuleFor(x => x.Google)
            .SetValidator(new GoogleOAuthOptionsValidator());
    }
}

public class GoogleOAuthOptionsValidator : AbstractValidator<GoogleOAuthOptions>
{
    public GoogleOAuthOptionsValidator()
    {
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage(ValidationMessages.OAuth.Google.ClientIdRequired);

            RuleFor(x => x.ClientSecret)
                .NotEmpty()
                .WithMessage(ValidationMessages.OAuth.Google.ClientSecretRequired);
        });
    }
}
