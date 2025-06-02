using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Discounts.Queries.GetDiscountsByVendor;

public record GetDiscountsByVendorQuery(long VendorId) : IQuery<List<DiscountDto>>;