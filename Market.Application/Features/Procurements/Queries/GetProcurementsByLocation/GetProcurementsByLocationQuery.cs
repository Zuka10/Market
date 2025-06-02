using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByLocation;

public record GetProcurementsByLocationQuery(long LocationId) : IQuery<List<ProcurementDto>>;