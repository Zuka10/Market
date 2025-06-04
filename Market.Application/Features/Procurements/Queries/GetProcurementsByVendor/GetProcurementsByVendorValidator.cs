using FluentValidation;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByVendor;

public class GetProcurementsByVendorValidator : AbstractValidator<GetProcurementsByVendorQuery>
{
    public GetProcurementsByVendorValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");
    }
}