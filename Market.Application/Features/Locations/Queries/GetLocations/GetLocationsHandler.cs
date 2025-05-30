using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Filters;

namespace Market.Application.Features.Locations.Queries.GetLocations;

public class GetLocationsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetLocationsQuery, PagedResult<LocationDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<PagedResult<LocationDto>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var filterParams = new LocationFilterParameters
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm?.Trim(),
            City = request.City?.Trim(),
            Country = request.Country?.Trim(),
            IsActive = request.IsActive,
            SortBy = request.SortBy?.Trim(),
            SortDirection = request.SortDirection?.Trim()?.ToLower()
        };

        var pagedLocations = await _unitOfWork.Locations.GetLocationsPagedAsync(filterParams);
        var locationDtos = _mapper.Map<List<LocationDto>>(pagedLocations.Items);

        var pagedResult = new PagedResult<LocationDto>
        {
            Items = locationDtos,
            TotalCount = pagedLocations.TotalCount,
            Page = pagedLocations.Page,
            PageSize = pagedLocations.PageSize,
            TotalPages = pagedLocations.TotalPages,
            HasNextPage = pagedLocations.HasNextPage,
            HasPreviousPage = pagedLocations.HasPreviousPage
        };

        return BaseResponse<PagedResult<LocationDto>>.Success(pagedResult, "Locations retrieved successfully.");
    }
}