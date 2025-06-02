using FluentValidation;

namespace Market.Application.Features.Vendors.Commands.DeactivateVendor;

public class DeactivateVendorValidator : AbstractValidator<DeactivateVendorCommand>
{
    public DeactivateVendorValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");
    }
}