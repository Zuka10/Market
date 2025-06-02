using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;
using Market.Domain.Filters;

namespace Market.Application.Features.Procurements.Queries.GetProcurements;

public record GetProcurementsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? ReferenceNo = null,
    long? VendorId = null,
    string? VendorName = null,
    long? LocationId = null,
    string? LocationName = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? Notes = null,
    string? SortBy = null,
    string? SortDirection = null
) : IQuery<PagedResult<ProcurementDto>>;