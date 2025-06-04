using FluentValidation;

namespace Market.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID must be greater than 0.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}