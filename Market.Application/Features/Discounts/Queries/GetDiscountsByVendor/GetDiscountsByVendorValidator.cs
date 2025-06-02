using FluentValidation;

namespace Market.Application.Features.Discounts.Queries.GetDiscountsByVendor;

public class GetDiscountsByVendorValidator : AbstractValidator<GetDiscountsByVendorQuery>
{
    public GetDiscountsByVendorValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");
    }
}