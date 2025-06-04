using FluentValidation;

namespace Market.Application.Features.Products.Queries.GetLowStockProducts;

public class GetLowStockProductsValidator : AbstractValidator<GetLowStockProductsQuery>
{
    public GetLowStockProductsValidator()
    {
        RuleFor(x => x.Threshold)
            .GreaterThan(0).WithMessage("Threshold must be greater than 0.")
            .LessThan(1000).WithMessage("Threshold cannot exceed 1000.");

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0.")
            .When(x => x.LocationId.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.")
            .When(x => x.CategoryId.HasValue);
    }
}