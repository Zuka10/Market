using FluentValidation;

namespace Market.Application.Features.VendorLocations.Queries.GetLocationsByVendor;

public class GetLocationsByVendorValidator : AbstractValidator<GetLocationsByVendorQuery>
{
    public GetLocationsByVendorValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");
    }
}