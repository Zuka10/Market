using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Orders.Queries.GetOrderByUser;

public record GetOrdersByUserQuery(long UserId) : IQuery<List<OrderDto>>;