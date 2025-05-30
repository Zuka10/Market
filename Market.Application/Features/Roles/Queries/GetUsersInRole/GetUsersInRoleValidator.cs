using FluentValidation;

namespace Market.Application.Features.Roles.Queries.GetUsersInRole;

public class GetUsersInRoleValidator : AbstractValidator<GetUsersInRoleQuery>
{
    public GetUsersInRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage("Role ID must be greater than 0.");
    }
}