using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Enums;

namespace Market.Application.Features.Orders.Queries.GetOrdersByStatus;

public record GetOrdersByStatusQuery(OrderStatus Status) : IQuery<List<OrderDto>>;