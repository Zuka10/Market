using FluentValidation;

namespace Market.Application.Features.Vendors.Commands.DeleteVendor;

public class DeleteVendorValidator : AbstractValidator<DeleteVendorCommand>
{
    public DeleteVendorValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");
    }
}