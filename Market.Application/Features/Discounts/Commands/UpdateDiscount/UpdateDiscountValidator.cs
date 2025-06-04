using FluentValidation;

namespace Market.Application.Features.Discounts.Commands.UpdateDiscount;

public class UpdateDiscountValidator : AbstractValidator<UpdateDiscountCommand>
{
    public UpdateDiscountValidator()
    {
        RuleFor(x => x.DiscountId)
            .GreaterThan(0).WithMessage("Discount ID must be greater than 0.");

        RuleFor(x => x.DiscountCode)
            .NotEmpty().WithMessage("Discount code is required.")
            .MaximumLength(20).WithMessage("Discount code cannot exceed 20 characters.")
            .Must(BeAValidDiscountCode).WithMessage("Discount code must contain only letters, numbers, and hyphens.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Percentage)
            .GreaterThan(0).WithMessage("Percentage must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Percentage cannot exceed 100.");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate < x.EndDate)
            .WithMessage("Start date must be earlier than end date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x)
            .Must(x => x.LocationId.HasValue || x.VendorId.HasValue)
            .WithMessage("Discount must be associated with either a location or a vendor.");

        RuleFor(x => x)
            .Must(x => !x.LocationId.HasValue || !x.VendorId.HasValue)
            .WithMessage("Discount cannot be associated with both a location and a vendor.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.")
            .When(x => x.VendorId.HasValue);
    }

    private static bool BeAValidDiscountCode(string code)
    {
        return !string.IsNullOrWhiteSpace(code) && System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Z0-9\-]+$");
    }
}