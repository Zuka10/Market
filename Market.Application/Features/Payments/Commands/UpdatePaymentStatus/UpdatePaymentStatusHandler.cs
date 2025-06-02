using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Payments.Commands.UpdatePaymentStatus;

public class UpdatePaymentStatusHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdatePaymentStatusCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var existingPayment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId);
        if (existingPayment is null)
        {
            return BaseResponse<bool>.Failure(["Payment not found."]);
        }

        existingPayment.Status = request.Status;
        existingPayment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Payments.UpdateAsync(existingPayment);

        return BaseResponse<bool>.Success(true, "Payment status updated successfully.");
    }
}