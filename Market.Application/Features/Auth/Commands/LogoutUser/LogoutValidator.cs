using FluentValidation;

namespace Market.Application.Features.Auth.Commands.LogoutUser;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .MinimumLength(10)
            .WithMessage("Invalid refresh token format.");
    }
}