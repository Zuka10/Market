using FluentValidation;

namespace Market.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Role ID must be greater than 0.");
    }
}