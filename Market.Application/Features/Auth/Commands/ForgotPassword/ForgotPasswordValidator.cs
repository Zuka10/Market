using FluentValidation;

namespace Market.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Valid email address is required.");
    }
}