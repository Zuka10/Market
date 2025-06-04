using FluentValidation;

namespace Market.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Product ID is required.");
    }
}