using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Orders.Queries.GetOrderByOrderNumber;

public class GetOrderByNumberHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrderByNumberQuery, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<OrderDto>> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(request.OrderNumber.Trim());
        if (order is null)
        {
            return BaseResponse<OrderDto>.Failure(["Order with this number not found."]);
        }

        var orderDto = _mapper.Map<OrderDto>(order);
        return BaseResponse<OrderDto>.Success(orderDto, "Order retrieved successfully.");
    }
}