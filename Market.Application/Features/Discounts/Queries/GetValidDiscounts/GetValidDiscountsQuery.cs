using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Discounts.Queries.GetValidDiscounts;

public record GetValidDiscountsQuery(
    long? LocationId = null,
    long? VendorId = null
) : IQuery<List<DiscountDto>>;