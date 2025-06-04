using FluentValidation;

namespace Market.Application.Features.VendorLocations.Commands.DeleteVendorLocation;

public class DeleteVendorLocationValidator : AbstractValidator<DeleteVendorLocationCommand>
{
    public DeleteVendorLocationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID must be greater than 0.");
    }
}