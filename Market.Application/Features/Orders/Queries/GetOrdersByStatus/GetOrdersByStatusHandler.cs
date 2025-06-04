using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Orders.Queries.GetOrdersByStatus;

public class GetOrdersByStatusHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrdersByStatusQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<OrderDto>>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByStatusAsync(request.Status);
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        return BaseResponse<List<OrderDto>>.Success(orderDtos, $"Found {orderDtos.Count} orders with status {request.Status}.");
    }
}