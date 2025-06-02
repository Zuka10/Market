using FluentValidation;

namespace Market.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.")
            .LessThan(1000000).WithMessage("Price cannot exceed $1,000,000.");

        RuleFor(x => x.InStock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.")
            .LessThan(1000000).WithMessage("Stock cannot exceed 1,000,000 units.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(20).WithMessage("Unit cannot exceed 20 characters.")
            .Must(BeAValidUnit).WithMessage("Invalid unit. Valid units are: piece, kg, gram, liter, ml, meter, cm, pack, box, dozen, pair");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.");
    }

    private static bool BeAValidUnit(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            return false;
        }

        var validUnits = new[] { "piece", "kg", "gram", "liter", "ml", "meter", "cm", "pack", "box", "dozen", "pair" };
        return validUnits.Contains(unit.ToLowerInvariant());
    }
}