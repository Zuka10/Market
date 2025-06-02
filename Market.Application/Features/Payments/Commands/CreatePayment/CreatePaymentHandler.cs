using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;

namespace Market.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdPayment = await _unitOfWork.Payments.AddAsync(payment);
        var paymentDto = _mapper.Map<PaymentDto>(createdPayment);

        return BaseResponse<PaymentDto>.Success(paymentDto, "Payment created successfully.");
    }
}