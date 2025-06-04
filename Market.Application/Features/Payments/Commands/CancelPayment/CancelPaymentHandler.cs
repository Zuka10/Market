using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Payments.Commands.CancelPayment;

public class CancelPaymentHandler(IUnitOfWork unitOfWork) : ICommandHandler<CancelPaymentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Payments.DeleteAsync(request.PaymentId);
        return BaseResponse<bool>.Success(true, "Payment cancelled successfully.");
    }
}