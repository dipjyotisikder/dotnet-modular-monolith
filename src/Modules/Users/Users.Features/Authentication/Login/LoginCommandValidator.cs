using FluentValidation;

namespace Users.Features.Authentication.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .Must(BeValidEmail).WithMessage("Email must be valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }

    private static bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Reject emails with whitespace characters (space, tab, newline, carriage return)
        if (email.Any(c => char.IsWhiteSpace(c)))
            return false;

        // Reject emails with invalid characters (comma, etc.)
        if (email.Contains(','))
            return false;

        // Reject emails with consecutive dots
        if (email.Contains(".."))
            return false;

        // Verify domain has at least one dot for TLD
        if (!email.Contains("@"))
            return false;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var domain = parts[1];
        if (!domain.Contains("."))
            return false;

        // Additional check: domain cannot end with just a dot
        if (domain.EndsWith(".") || domain.StartsWith("."))
            return false;

        // Basic email format validation
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
