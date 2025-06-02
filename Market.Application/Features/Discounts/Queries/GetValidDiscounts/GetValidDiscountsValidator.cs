using FluentValidation;

namespace Market.Application.Features.Discounts.Queries.GetValidDiscounts;

public class GetValidDiscountsValidator : AbstractValidator<GetValidDiscountsQuery>
{
    public GetValidDiscountsValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.")
            .When(x => x.VendorId.HasValue);
    }
}