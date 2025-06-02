using FluentValidation;

namespace Market.Application.Features.VendorLocations.Commands.UpdateVendorLocation;

public class UpdateVendorLocationValidator : AbstractValidator<UpdateVendorLocationCommand>
{
    public UpdateVendorLocationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID must be greater than 0.");

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");

        RuleFor(x => x.StallNumber)
            .MaximumLength(20).WithMessage("Stall number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.StallNumber));

        RuleFor(x => x.RentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Rent amount cannot be negative.")
            .LessThan(1000000).WithMessage("Rent amount cannot exceed $1,000,000.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || x.EndDate > x.StartDate)
            .WithMessage("End date must be later than start date.")
            .When(x => x.EndDate.HasValue);
    }
}