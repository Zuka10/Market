using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Procurements.Queries.GetProcurements;

public class GetAllProcurementsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetProcurementsQuery, PagedResult<ProcurementDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<ProcurementDto>>> Handle(GetProcurementsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new ProcurementFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm?.Trim(),
            ReferenceNo = request.ReferenceNo?.Trim(),
            VendorId = request.VendorId,
            VendorName = request.VendorName?.Trim(),
            LocationId = request.LocationId,
            LocationName = request.LocationName?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            Notes = request.Notes?.Trim(),
            SortBy = request.SortBy?.Trim(),
            SortDirection = request.SortDirection?.Trim()?.ToLower()
        };

        var pagedProcurements = await _unitOfWork.Procurements.GetProcurementsAsync(filterParams);
        var procurementDtos = _mapper.Map<List<ProcurementDto>>(pagedProcurements.Items);

        var pagedResult = new PagedResult<ProcurementDto>
        {
            Items = procurementDtos,
            TotalCount = pagedProcurements.TotalCount,
            Page = pagedProcurements.Page,
            PageSize = pagedProcurements.PageSize,
            TotalPages = pagedProcurements.TotalPages,
            HasNextPage = pagedProcurements.HasNextPage,
            HasPreviousPage = pagedProcurements.HasPreviousPage
        };

        return BaseResponse<PagedResult<ProcurementDto>>.Success(pagedResult, $"Retrieved {pagedResult.TotalCount} procurements successfully.");
    }
}