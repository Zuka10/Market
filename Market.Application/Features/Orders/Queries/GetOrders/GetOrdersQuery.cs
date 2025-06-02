using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Enums;
using Market.Domain.Filters;

namespace Market.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    OrderStatus? Status = null,
    long? UserId = null,
    long? LocationId = null,
    long? DiscountId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    decimal? MinTotal = null,
    decimal? MaxTotal = null,
    string? CustomerName = null,
    string? SortBy = null,
    string? SortDirection = null
) : IQuery<PagedResult<OrderDto>>;