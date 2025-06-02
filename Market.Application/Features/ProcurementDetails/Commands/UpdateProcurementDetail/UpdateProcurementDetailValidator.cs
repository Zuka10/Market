using FluentValidation;

namespace Market.Application.Features.ProcurementDetails.Commands.UpdateProcurementDetail;

public class UpdateProcurementDetailValidator : AbstractValidator<UpdateProcurementDetailCommand>
{
    public UpdateProcurementDetailValidator()
    {
        RuleFor(x => x.ProcurementDetailId)
            .GreaterThan(0).WithMessage("Procurement detail ID must be greater than 0.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThan(0).WithMessage("Purchase price must be greater than 0.")
            .LessThan(100000).WithMessage("Purchase price cannot exceed $100,000.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThan(100000).WithMessage("Quantity cannot exceed 100,000.");

        RuleFor(x => x.LineTotal)
            .GreaterThan(0).WithMessage("Line total must be greater than 0.")
            .LessThan(10000000).WithMessage("Line total cannot exceed $10,000,000.");

        // Business rule: Line total should equal purchase price × quantity
        RuleFor(x => x)
            .Must(x => Math.Abs(x.LineTotal - (x.PurchasePrice * x.Quantity)) < 0.01m)
            .WithMessage("Line total must equal purchase price × quantity.");
    }
}