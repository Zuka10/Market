using FluentValidation;

namespace Market.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID is required.");
    }
}