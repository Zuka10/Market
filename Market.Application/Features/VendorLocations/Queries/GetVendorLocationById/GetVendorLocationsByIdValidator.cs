using FluentValidation;

namespace Market.Application.Features.VendorLocations.Queries.GetVendorLocationById;

public class GetVendorLocationByIdValidator : AbstractValidator<GetVendorLocationByIdQuery>
{
    public GetVendorLocationByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID must be greater than 0.");
    }
}