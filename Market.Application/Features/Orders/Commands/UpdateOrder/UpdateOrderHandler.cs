using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Orders.Commands.UpdateOrder;

public class UpdateOrderHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (existingOrder is null)
        {
            return BaseResponse<bool>.Failure(["Order not found."]);
        }

        existingOrder.OrderNumber = request.OrderNumber.Trim();
        existingOrder.OrderDate = request.OrderDate;
        existingOrder.Total = request.Total;
        existingOrder.SubTotal = request.SubTotal;
        existingOrder.TotalCommission = request.TotalCommission;
        existingOrder.Status = request.Status;
        existingOrder.LocationId = request.LocationId;
        existingOrder.DiscountId = request.DiscountId;
        existingOrder.DiscountAmount = request.DiscountAmount;
        existingOrder.UserId = request.UserId;
        existingOrder.CustomerName = request.CustomerName?.Trim();
        existingOrder.CustomerPhone = request.CustomerPhone?.Trim();
        existingOrder.Notes = request.Notes?.Trim();
        existingOrder.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(existingOrder);
        return BaseResponse<bool>.Success(true, "Order updated successfully.");
    }
}