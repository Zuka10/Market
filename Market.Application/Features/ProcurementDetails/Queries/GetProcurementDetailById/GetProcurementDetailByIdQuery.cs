using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.ProcurementDetails.Queries.GetProcurementDetailById;

public record GetProcurementDetailByIdQuery(long Id) : IQuery<ProcurementDetailDto>;