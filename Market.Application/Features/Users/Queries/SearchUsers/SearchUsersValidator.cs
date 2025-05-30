using FluentValidation;

namespace Market.Application.Features.Users.Queries.SearchUsers;

public class SearchUsersValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Search term is required.")
            .MinimumLength(2)
            .WithMessage("Search term must be at least 2 characters long.")
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.");
    }
}