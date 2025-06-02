using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Orders.Queries.GetOrdersByDateRange;

public class GetOrdersByDateRangeHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrdersByDateRangeQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<OrderDto>>> Handle(GetOrdersByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var pagedOrders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(request.StartDate, request.EndDate);
        var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders);

        return BaseResponse<List<OrderDto>>.Success(orderDtos, $"Found {orderDtos.Count} orders in date range.");
    }
}