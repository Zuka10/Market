using FluentValidation;

namespace Market.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MinimumLength(2)
            .WithMessage("Role name must be at least 2 characters long.")
            .MaximumLength(50)
            .WithMessage("Role name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s-]+$")
            .WithMessage("Role name can only contain letters, spaces, and hyphens.");
    }
}