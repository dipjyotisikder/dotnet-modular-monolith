using FluentValidation;

namespace Shared.Infrastructure.Configuration.Options.Validation;

public class OutboxOptionsValidator : AbstractValidator<OutboxOptions>, IOptionsValidator
{
    public OutboxOptionsValidator()
    {
        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(1, 20)
            .WithMessage(ValidationMessages.Outbox.MaxRetries);

        RuleFor(x => x.BatchSize)
            .InclusiveBetween(1, 1000)
            .WithMessage(ValidationMessages.Outbox.BatchSize);

        RuleFor(x => x.PollingIntervalSeconds)
            .InclusiveBetween(1, 60)
            .WithMessage(ValidationMessages.Outbox.PollingIntervalSeconds);

        RuleFor(x => x.RetentionDays)
            .InclusiveBetween(1, 90)
            .WithMessage(ValidationMessages.Outbox.RetentionDays);

        RuleFor(x => x.CleanupIntervalHours)
            .InclusiveBetween(1, 168)
            .WithMessage(ValidationMessages.Outbox.CleanupIntervalHours);
    }
}
