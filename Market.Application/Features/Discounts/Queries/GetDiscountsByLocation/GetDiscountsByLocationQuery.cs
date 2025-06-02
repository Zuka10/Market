using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Discounts.Queries.GetDiscountsByLocation;

public record GetDiscountsByLocationQuery(long LocationId) : IQuery<List<DiscountDto>>;