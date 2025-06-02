using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Procurements.Queries.GetProcurementsByDateRange;

public record GetProcurementsByDateRangeQuery(
    DateTime StartDate,
    DateTime EndDate
) : IQuery<List<ProcurementDto>>;