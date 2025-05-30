using FluentValidation;

namespace Market.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdValidator : AbstractValidator<GetRoleByIdQuery>
{
    public GetRoleByIdValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage("Role ID must be greater than 0.");
    }
}