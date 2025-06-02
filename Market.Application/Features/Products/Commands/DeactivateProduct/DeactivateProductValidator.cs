using FluentValidation;

namespace Market.Application.Features.Products.Commands.DeactivateProduct;

public class DeactivateProductValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0.");
    }
}