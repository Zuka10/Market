using FluentValidation;

namespace Market.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}