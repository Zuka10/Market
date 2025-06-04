using FluentValidation;

namespace Market.Application.Features.Products.Queries.GetOutOfStockProducts;

public class GetOutOfStockProductsValidator : AbstractValidator<GetOutOfStockProductsQuery>
{
    public GetOutOfStockProductsValidator()
    {
        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.")
            .When(x => x.CategoryId.HasValue);
    }
}