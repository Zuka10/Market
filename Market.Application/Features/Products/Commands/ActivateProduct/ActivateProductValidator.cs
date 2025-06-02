using FluentValidation;

namespace Market.Application.Features.Products.Commands.ActivateProduct;

public class ActivateProductValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");
    }
}