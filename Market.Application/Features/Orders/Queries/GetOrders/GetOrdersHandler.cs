using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Orders.Queries.GetOrders;

public class GetAllOrdersHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new OrderFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm?.Trim(),
            Status = request.Status,
            UserId = request.UserId,
            LocationId = request.LocationId,
            DiscountId = request.DiscountId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinTotal = request.MinTotal,
            MaxTotal = request.MaxTotal,
            CustomerName = request.CustomerName?.Trim(),
            SortBy = request.SortBy?.Trim(),
            SortDirection = request.SortDirection?.Trim()?.ToLower()
        };

        var pagedOrders = await _unitOfWork.Orders.GetOrdersAsync(filterParams);
        var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders.Items);

        var pagedResult = new PagedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = pagedOrders.TotalCount,
            Page = pagedOrders.Page,
            PageSize = pagedOrders.PageSize,
            TotalPages = pagedOrders.TotalPages,
            HasNextPage = pagedOrders.HasNextPage,
            HasPreviousPage = pagedOrders.HasPreviousPage
        };

        return BaseResponse<PagedResult<OrderDto>>.Success(pagedResult, $"Retrieved {pagedResult.TotalCount} orders successfully.");
    }
}