using FluentValidation;

namespace Market.Application.Features.VendorLocations.Queries.GetVendorsByLocation;

public class GetVendorsByLocationValidator : AbstractValidator<GetVendorsByLocationQuery>
{
    public GetVendorsByLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");
    }
}