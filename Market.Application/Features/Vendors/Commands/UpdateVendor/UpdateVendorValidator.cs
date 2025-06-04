using FluentValidation;

namespace Market.Application.Features.Vendors.Commands.UpdateVendor;

public class UpdateVendorValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.ContactPerson)
            .MaximumLength(100).WithMessage("Contact person cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.ContactPerson));
    }
}