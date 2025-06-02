using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Vendors.Queries.GetVendorByEmail;

public record GetVendorByEmailQuery(string Email) : IQuery<VendorDto>;