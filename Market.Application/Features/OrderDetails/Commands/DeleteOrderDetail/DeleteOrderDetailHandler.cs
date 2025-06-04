using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.OrderDetails.Commands.DeleteOrderDetail;

public class DeleteOrderDetailHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteOrderDetailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteOrderDetailCommand request, CancellationToken cancellationToken)
    {
        var existingOrderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(request.OrderDetailId);
        if (existingOrderDetail is null)
        {
            return BaseResponse<bool>.Failure(["Order detail not found."]);
        }

        // Check if this is the last item in the order
        var orderDetails = await _unitOfWork.OrderDetails.GetByOrderAsync(existingOrderDetail.OrderId);
        if (orderDetails.Count() <= 1)
        {
            return BaseResponse<bool>.Failure(["Cannot remove the last item from an order. Cancel the entire order instead."]);
        }

        // Store values for order total recalculation
        var lineTotal = existingOrderDetail.LineTotal;
        var orderId = existingOrderDetail.OrderId;

        await _unitOfWork.OrderDetails.DeleteAsync(request.OrderDetailId);

        // Update order totals
        await UpdateOrderTotalsAsync(orderId, -lineTotal);

        return BaseResponse<bool>.Success(true, "Order detail removed successfully.");
    }

    private async Task UpdateOrderTotalsAsync(long orderId, decimal lineTotalDifference)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order is not null)
        {
            order.SubTotal += lineTotalDifference;
            order.Total = order.SubTotal - order.DiscountAmount;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order);
        }
    }
}