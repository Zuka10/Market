using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.OrderDetails.Commands.UpdateOrderDetail;

public class UpdateOrderDetailHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateOrderDetailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateOrderDetailCommand request, CancellationToken cancellationToken)
    {
        var existingOrderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(request.OrderDetailId);
        if (existingOrderDetail is null)
        {
            return BaseResponse<bool>.Failure(["Order detail not found."]);
        }

        // Verify the product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product is null)
        {
            return BaseResponse<bool>.Failure(["Product not found."]);
        }

        // Store original values for order total recalculation
        var originalLineTotal = existingOrderDetail.LineTotal;

        // Update order detail properties
        existingOrderDetail.ProductId = request.ProductId;
        existingOrderDetail.Quantity = request.Quantity;
        existingOrderDetail.UnitPrice = request.UnitPrice;
        existingOrderDetail.LineTotal = request.LineTotal;
        existingOrderDetail.CostPrice = request.CostPrice;
        existingOrderDetail.Profit = request.Profit;

        await _unitOfWork.OrderDetails.UpdateAsync(existingOrderDetail);

        // Update order totals if line total changed
        if (Math.Abs(originalLineTotal - request.LineTotal) > 0.01m)
        {
            await UpdateOrderTotalsAsync(existingOrderDetail.OrderId, request.LineTotal - originalLineTotal);
        }

        return BaseResponse<bool>.Success(true, "Order detail updated successfully.");
    }

    private async Task UpdateOrderTotalsAsync(long orderId, decimal lineTotal)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order is not null)
        {
            order.SubTotal += lineTotal;
            order.Total = order.SubTotal - order.DiscountAmount;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order);
        }
    }
}