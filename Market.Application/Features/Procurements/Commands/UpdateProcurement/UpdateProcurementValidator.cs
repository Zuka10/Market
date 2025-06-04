using FluentValidation;

namespace Market.Application.Features.Procurements.Commands.UpdateProcurement;

public class UpdateProcurementValidator : AbstractValidator<UpdateProcurementCommand>
{
    public UpdateProcurementValidator()
    {
        RuleFor(x => x.ProcurementId)
            .GreaterThan(0).WithMessage("Procurement ID must be greater than 0.");

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor ID must be greater than 0.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");

        RuleFor(x => x.ReferenceNo)
            .NotEmpty().WithMessage("Reference number is required.")
            .MaximumLength(50).WithMessage("Reference number cannot exceed 50 characters.")
            .Must(BeAValidReferenceFormat).WithMessage("Reference number must follow format: XX-NNNN or XXX-NNNNNN (e.g., PR-001234).");

        RuleFor(x => x.ProcurementDate)
            .NotEmpty().WithMessage("Procurement date is required.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0.")
            .LessThan(10000000).WithMessage("Total amount cannot exceed $10,000,000.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private static bool BeAValidReferenceFormat(string referenceNo)
    {
        return !string.IsNullOrWhiteSpace(referenceNo)
            && System.Text.RegularExpressions.Regex.IsMatch(referenceNo, @"^[A-Z]{2,3}-\d{4,6}$");
    }
}