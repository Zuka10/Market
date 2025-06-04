using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailsByProduct;

public record GetProcurementDetailsByProductQuery(long ProductId) : IQuery<List<ProcurementDetailDto>>;