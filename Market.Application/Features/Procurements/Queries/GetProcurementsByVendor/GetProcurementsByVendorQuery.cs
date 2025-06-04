using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByVendor;

public record GetProcurementsByVendorQuery(long VendorId) : IQuery<List<ProcurementDto>>;