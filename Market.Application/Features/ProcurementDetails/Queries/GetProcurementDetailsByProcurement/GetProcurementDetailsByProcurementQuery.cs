using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProcurement;

public record GetProcurementDetailsByProcurementQuery(
    long ProcurementId,
    bool IncludeProductDetails = true
) : IQuery<List<ProcurementDetailDto>>;