using FluentValidation;

namespace Market.Application.Features.Categories.Queries.SearchCategories;

public class SearchCategoriesValidator : AbstractValidator<SearchCategoriesQuery>
{
    public SearchCategoriesValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Search term cannot be empty.")
            .MinimumLength(3).WithMessage("Search term must be at least 3 characters long.")
            .MaximumLength(100).WithMessage("Search term must not exceed 100 characters.");
    }
}