using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (existingOrder is null)
        {
            return BaseResponse<bool>.Failure(["Order not found."]);
        }

        existingOrder.Status = request.NewStatus;
        existingOrder.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(existingOrder);
        return BaseResponse<bool>.Success(true, $"Order status updated to {request.NewStatus} successfully.");
    }
}