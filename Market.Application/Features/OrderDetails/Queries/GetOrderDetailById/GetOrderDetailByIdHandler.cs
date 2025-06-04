using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailById;

public class GetOrderDetailByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrderDetailByIdQuery, OrderDetailDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<OrderDetailDto>> Handle(GetOrderDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var orderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(request.Id);
        if (orderDetail is null)
        {
            return BaseResponse<OrderDetailDto>.Failure(["Order detail not found."]);
        }

        var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
        return BaseResponse<OrderDetailDto>.Success(orderDetailDto, "Order detail retrieved successfully.");
    }
}