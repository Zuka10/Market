using FluentValidation;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByProduct;

public class GetOrderDetailsByProductValidator : AbstractValidator<GetOrderDetailsByProductQuery>
{
    public GetOrderDetailsByProductValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");
    }
}