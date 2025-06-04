using FluentValidation;

namespace Market.Application.Features.Products.Commands.UpdateProductStock;

public class UpdateProductStockValidator : AbstractValidator<UpdateProductStockCommand>
{
    public UpdateProductStockValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.NewStock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.")
            .LessThan(1000000).WithMessage("Stock cannot exceed 1,000,000 units.");
    }
}