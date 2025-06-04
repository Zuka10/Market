using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Vendors.Queries.GetVendors;

public class GetVendorsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetVendorsQuery, PagedResult<VendorDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<VendorDto>>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new VendorFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm?.Trim(),
            Email = request.Email?.Trim(),
            ContactPersonName = request.ContactPersonName?.Trim(),
            IsActive = request.IsActive,
            MinCommissionRate = request.MinCommissionRate,
            MaxCommissionRate = request.MaxCommissionRate,
            LocationId = request.LocationId,
            SortBy = request.SortBy?.Trim(),
            SortDirection = request.SortDirection?.Trim()?.ToLower()
        };

        var pagedVendors = await _unitOfWork.Vendors.GetVendorsAsync(filterParams);
        var vendorDtos = _mapper.Map<List<VendorDto>>(pagedVendors.Items);

        var pagedResult = new PagedResult<VendorDto>
        {
            Items = vendorDtos,
            TotalCount = pagedVendors.TotalCount,
            Page = pagedVendors.Page,
            PageSize = pagedVendors.PageSize,
            TotalPages = pagedVendors.TotalPages,
            HasNextPage = pagedVendors.HasNextPage,
            HasPreviousPage = pagedVendors.HasPreviousPage
        };

        var message = BuildSuccessMessage(request, pagedResult.TotalCount);
        return BaseResponse<PagedResult<VendorDto>>.Success(pagedResult, message);
    }

    private static string BuildSuccessMessage(GetVendorsQuery request, int totalCount)
    {
        if (HasAnyFilterCriteria(request))
        {
            return $"Retrieved {totalCount} vendors matching the filter criteria.";
        }

        return $"Retrieved {totalCount} vendors successfully.";
    }

    private static bool HasAnyFilterCriteria(GetVendorsQuery request)
    {
        return !string.IsNullOrWhiteSpace(request.SearchTerm) ||
               !string.IsNullOrWhiteSpace(request.Email) ||
               !string.IsNullOrWhiteSpace(request.ContactPersonName) ||
               request.IsActive.HasValue ||
               request.MinCommissionRate.HasValue ||
               request.MaxCommissionRate.HasValue ||
               request.LocationId.HasValue;
    }
}