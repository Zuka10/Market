using FluentValidation;

namespace Market.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");
    }
}