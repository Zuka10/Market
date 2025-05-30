using FluentValidation;
using Market.Application.Features.Users.Queries.GetUsersByRole;

namespace Market.Application.Features.Users.Queries.GetUsersByRoleId;

public class GetUsersByRoleValidator : AbstractValidator<GetUsersByRoleQuery>
{
    public GetUsersByRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage("Role ID is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.SearchTerm)
            .MinimumLength(2)
            .WithMessage("Search term must be at least 2 characters long.")
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }
}