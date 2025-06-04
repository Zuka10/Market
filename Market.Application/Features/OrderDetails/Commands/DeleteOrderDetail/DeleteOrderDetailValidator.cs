using FluentValidation;

namespace Market.Application.Features.OrderDetails.Commands.DeleteOrderDetail;

public class DeleteOrderDetailValidator : AbstractValidator<DeleteOrderDetailCommand>
{
    public DeleteOrderDetailValidator()
    {
        RuleFor(x => x.OrderDetailId)
            .GreaterThan(0).WithMessage("Order detail ID must be greater than 0.");
    }
}