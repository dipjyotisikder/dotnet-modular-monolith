using FluentValidation;

namespace Users.Features.Authentication.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required");
    }
}
