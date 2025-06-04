using FluentValidation;

namespace Market.Application.Features.Products.Queries.GetProductsByCategory;

public class GetProductsByCategoryValidator : AbstractValidator<GetProductsByCategoryQuery>
{
    public GetProductsByCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.");
    }
}