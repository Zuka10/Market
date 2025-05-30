using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Locations.Queries.SearchLocations;

public class SearchLocationsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<SearchLocationsQuery, List<LocationDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<List<LocationDto>>> Handle(SearchLocationsQuery request, CancellationToken cancellationToken)
    {
        var locations = await _unitOfWork.Locations.SearchLocationsAsync(request.SearchTerm);

        var locationDtos = _mapper.Map<List<LocationDto>>(locations);

        return BaseResponse<List<LocationDto>>.Success(locationDtos, $"Found {locationDtos.Count} locations matching '{request.SearchTerm}'.");
    }
}