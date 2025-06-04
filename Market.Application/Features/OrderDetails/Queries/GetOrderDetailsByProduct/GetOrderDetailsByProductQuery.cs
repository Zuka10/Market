using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByProduct;

public record GetOrderDetailsByProductQuery(
    long ProductId
) : IQuery<List<OrderDetailDto>>;