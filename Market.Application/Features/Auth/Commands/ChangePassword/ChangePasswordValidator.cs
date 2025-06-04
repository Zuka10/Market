using FluentValidation;

namespace Market.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters long.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("New password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword)
            .WithMessage("Password confirmation does not match.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID is required.");
    }
}