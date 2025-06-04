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

        var pagedLocations = await _unitOfWork.Locations.GetLocationsAsync(filterParams);
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

        var message = BuildSuccessMessage(request, pagedResult.TotalCount);
        return BaseResponse<PagedResult<LocationDto>>.Success(pagedResult, message);
    }

    private static string BuildSuccessMessage(GetLocationsQuery request, int totalCount)
    {
        if (HasAnyFilterCriteria(request))
        {
            return $"Retrieved {totalCount} vendors matching the filter criteria.";
        }

        return $"Retrieved {totalCount} vendors successfully.";
    }

    private static bool HasAnyFilterCriteria(GetLocationsQuery request)
    {
        return !string.IsNullOrWhiteSpace(request.SearchTerm) ||
               !string.IsNullOrWhiteSpace(request.City) ||
               !string.IsNullOrWhiteSpace(request.Country) ||
               !string.IsNullOrWhiteSpace(request.SortDirection) ||
               !string.IsNullOrWhiteSpace(request.SortBy) ||
               request.IsActive.HasValue;
    }
}