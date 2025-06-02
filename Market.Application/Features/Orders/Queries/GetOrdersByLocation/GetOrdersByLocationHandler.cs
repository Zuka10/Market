using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Orders.Queries.GetOrdersByLocation;

public class GetOrdersByLocationHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrdersByLocationQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<OrderDto>>> Handle(GetOrdersByLocationQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByLocationAsync(request.LocationId);
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        return BaseResponse<List<OrderDto>>.Success(orderDtos, $"Found {orderDtos.Count} orders for location.");
    }
}