using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderHandler(IUnitOfWork unitOfWork) : ICommandHandler<CancelOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (existingOrder is null)
        {
            return BaseResponse<bool>.Failure(["Order not found."]);
        }

        await _unitOfWork.Orders.DeleteAsync(request.OrderId);
        return BaseResponse<bool>.Success(true, "Order cancelled successfully.");
    }
}