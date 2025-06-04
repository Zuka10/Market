using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Procurements.Queries.GetProcurementById;

public record GetProcurementByIdQuery(long Id) : IQuery<ProcurementDto>;