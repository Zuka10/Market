using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Orders.Queries.GetOrdersByDateRange;

public record GetOrdersByDateRangeQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<List<OrderDto>>;