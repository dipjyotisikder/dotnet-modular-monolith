using FluentValidation;

namespace Users.Features.Authentication.RegisterWithPassword;

public class RegisterWithPasswordCommandValidator : AbstractValidator<RegisterWithPasswordCommand>
{
    public RegisterWithPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letters")
            .Matches(@"[a-z]").WithMessage("Password must contain lowercase letters")
            .Matches(@"[0-9]").WithMessage("Password must contain numbers");
    }
}
