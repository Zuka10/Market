using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(long Id) : IQuery<OrderDto>;