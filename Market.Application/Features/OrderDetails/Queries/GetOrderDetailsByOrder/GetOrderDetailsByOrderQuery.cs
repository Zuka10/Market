using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.OrderDetails.Queries.GetOrderDetailsByOrder;

public record GetOrderDetailsByOrderQuery(
    long OrderId,
    bool IncludeProductDetails = true
) : IQuery<List<OrderDetailDto>>;