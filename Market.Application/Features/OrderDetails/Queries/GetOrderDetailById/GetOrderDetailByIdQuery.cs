using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailById;

public record GetOrderDetailByIdQuery(long Id) : IQuery<OrderDetailDto>;