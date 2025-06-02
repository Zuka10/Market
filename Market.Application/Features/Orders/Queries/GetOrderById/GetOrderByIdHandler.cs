using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(request.Id);
        if (order is null)
        {
            return BaseResponse<OrderDto>.Failure(["Order not found."]);
        }

        var orderDto = _mapper.Map<OrderDto>(order);
        return BaseResponse<OrderDto>.Success(orderDto, "Order retrieved successfully.");
    }
}