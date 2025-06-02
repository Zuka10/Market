using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdatePaymentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var existingPayment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId);
        if (existingPayment is null)
        {
            return BaseResponse<bool>.Failure(["Payment not found."]);
        }

        // Update payment properties
        existingPayment.Amount = request.Amount;
        existingPayment.PaymentMethod = request.PaymentMethod;
        existingPayment.PaymentDate = request.PaymentDate;
        existingPayment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Payments.UpdateAsync(existingPayment);

        return BaseResponse<bool>.Success(true, "Payment updated successfully.");
    }
}