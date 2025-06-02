using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetPaymentByIdQuery, PaymentDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id);
        if (payment is null)
        {
            return BaseResponse<PaymentDto>.Failure(["Payment not found."]);
        }

        var paymentDto = _mapper.Map<PaymentDto>(payment);
        return BaseResponse<PaymentDto>.Success(paymentDto, "Payment retrieved successfully.");
    }
}