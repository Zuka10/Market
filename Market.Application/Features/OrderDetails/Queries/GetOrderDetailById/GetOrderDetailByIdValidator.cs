using FluentValidation;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailById;

public class GetOrderDetailByIdValidator : AbstractValidator<GetOrderDetailByIdQuery>
{
    public GetOrderDetailByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Order detail ID must be greater than 0.");
    }
}