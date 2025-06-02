using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Discounts.Queries.GetDiscountById;

public record GetDiscountByIdQuery(long Id) : IQuery<DiscountDto>;