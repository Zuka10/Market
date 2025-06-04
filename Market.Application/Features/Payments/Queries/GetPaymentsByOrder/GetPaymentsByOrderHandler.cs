using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Payments.Queries.GetPaymentsByOrder;

public class GetPaymentsByOrderHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetPaymentsByOrderQuery, List<PaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<PaymentDto>>> Handle(GetPaymentsByOrderQuery request, CancellationToken cancellationToken)
    {
        var payments = await _unitOfWork.Payments.GetByOrderAsync(request.OrderId);
        var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);

        return BaseResponse<List<PaymentDto>>.Success(paymentDtos, $"Found {paymentDtos.Count} payments for order.");
    }
}