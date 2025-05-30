using FluentValidation;

namespace Market.Application.Features.Locations.Queries.GetLocationById;

public class GetLocationByIdValidator : AbstractValidator<GetLocationByIdQuery>
{
    public GetLocationByIdValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0)
            .WithMessage("Location ID is required.");
    }
}