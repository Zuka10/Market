using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Orders.Queries.GetOrderByOrderNumber;

public record GetOrderByNumberQuery(string OrderNumber) : IQuery<OrderDto>;