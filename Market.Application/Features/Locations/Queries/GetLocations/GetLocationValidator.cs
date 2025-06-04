using FluentValidation;

namespace Market.Application.Features.Locations.Queries.GetLocations;

public class GetLocationValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.");

        RuleFor(x => x.City)
            .MaximumLength(50)
            .WithMessage("City cannot exceed 50 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(50)
            .WithMessage("Country cannot exceed 50 characters.");

        RuleFor(x => x.SortBy)
            .Must(value => string.IsNullOrEmpty(value) || value == "Name" || value == "City" || value == "Country")
            .WithMessage("SortBy must be either 'Name', 'City', 'Country' or null.");

        RuleFor(x => x.SortDirection)
            .Must(value => string.IsNullOrEmpty(value) || value == "asc" || value == "desc")
            .WithMessage("SortDirection must be either 'asc', 'desc' or null.");
    }
}