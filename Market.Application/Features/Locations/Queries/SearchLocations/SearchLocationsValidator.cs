using FluentValidation;

namespace Market.Application.Features.Locations.Queries.SearchLocations;

public class SearchLocationsValidator : AbstractValidator<SearchLocationsQuery>
{
    public SearchLocationsValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Search term is required.")
            .MaximumLength(100)
            .WithMessage("Search term must not exceed 100 characters.");
    }
}