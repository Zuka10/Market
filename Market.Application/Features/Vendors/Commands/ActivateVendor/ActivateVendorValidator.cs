using FluentValidation;

namespace Market.Application.Features.Vendors.Commands.ActivateVendor;

public class ActivateVendorValidator : AbstractValidator<ActivateVendorCommand>
{
    public ActivateVendorValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");
    }
}