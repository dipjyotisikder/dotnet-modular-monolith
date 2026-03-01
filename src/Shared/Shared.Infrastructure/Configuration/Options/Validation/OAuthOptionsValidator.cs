using FluentValidation;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public class OAuthOptionsValidator : AbstractValidator<OAuthOptions>
{
    public OAuthOptionsValidator()
    {
        RuleFor(x => x.Google)
            .SetValidator(new GoogleOAuthOptionsValidator());

        RuleFor(x => x.Microsoft)
            .SetValidator(new MicrosoftOAuthOptionsValidator());

        RuleFor(x => x.GitHub)
            .SetValidator(new GitHubOAuthOptionsValidator());
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

public class MicrosoftOAuthOptionsValidator : AbstractValidator<MicrosoftOAuthOptions>
{
    public MicrosoftOAuthOptionsValidator()
    {
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage(ValidationMessages.OAuth.Microsoft.ClientIdRequired);

            RuleFor(x => x.ClientSecret)
                .NotEmpty()
                .WithMessage(ValidationMessages.OAuth.Microsoft.ClientSecretRequired);
        });
    }
}

public class GitHubOAuthOptionsValidator : AbstractValidator<GitHubOAuthOptions>
{
    public GitHubOAuthOptionsValidator()
    {
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage(ValidationMessages.OAuth.GitHub.ClientIdRequired);

            RuleFor(x => x.ClientSecret)
                .NotEmpty()
                .WithMessage(ValidationMessages.OAuth.GitHub.ClientSecretRequired);
        });
    }
}
