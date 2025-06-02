
using FluentValidation;

namespace Market.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID is required.");
    }
}