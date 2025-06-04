using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Orders.Queries.GetOrdersByLocation;

public record GetOrdersByLocationQuery(long LocationId) : IQuery<List<OrderDto>>;