using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Procurements.Queries.GetProcurementByReference;

public record GetProcurementByReferenceQuery(string ReferenceNo) : IQuery<ProcurementDto>;