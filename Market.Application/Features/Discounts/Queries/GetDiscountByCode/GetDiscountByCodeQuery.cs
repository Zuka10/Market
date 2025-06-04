using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Discounts.Queries.GetDiscountByCode;

public record GetDiscountByCodeQuery(string Code) : IQuery<DiscountDto>;