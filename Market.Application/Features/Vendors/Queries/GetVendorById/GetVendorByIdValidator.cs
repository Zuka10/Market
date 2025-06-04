using FluentValidation;

namespace Market.Application.Features.Vendors.Queries.GetVendorById;

public class GetVendorByIdValidator : AbstractValidator<GetVendorByIdQuery>
{
    public GetVendorByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");
    }
}