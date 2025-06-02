using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Orders.Commands.ApplyDiscountToOrder;

public class ApplyDiscountToOrderHandler(IUnitOfWork unitOfWork) : ICommandHandler<ApplyDiscountToOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(ApplyDiscountToOrderCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (existingOrder is null)
        {
            return BaseResponse<bool>.Failure(["Order not found."]);
        }

        if (request.DiscountAmount > existingOrder.SubTotal)
        {
            return BaseResponse<bool>.Failure(["Discount amount cannot exceed order subtotal."]);
        }

        existingOrder.DiscountId = request.DiscountId;
        existingOrder.DiscountAmount = request.DiscountAmount;
        existingOrder.Total = existingOrder.SubTotal - request.DiscountAmount;
        existingOrder.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(existingOrder);
        return BaseResponse<bool>.Success(true, "Discount applied to order successfully.");
    }
}