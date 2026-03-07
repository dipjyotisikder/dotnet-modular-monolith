using FluentValidation;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public class RabbitMqOptionsValidator : AbstractValidator<RabbitMqOptions>, IOptionsValidator
{
    public RabbitMqOptionsValidator()
    {
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.Host)
                .NotEmpty()
                .WithMessage(ValidationMessages.RabbitMq.HostRequired);

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage(ValidationMessages.RabbitMq.UsernameRequired);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(ValidationMessages.RabbitMq.PasswordRequired);

            RuleFor(x => x.Exchange)
                .NotEmpty()
                .WithMessage(ValidationMessages.RabbitMq.ExchangeRequired);
        });

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage(ValidationMessages.RabbitMq.Port);
    }
}
