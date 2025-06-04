using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Locations.Queries.GetLocationById;

public class GetLocationByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetLocationByIdQuery, LocationDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<LocationDto>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
        if (location == null)
        {
            return BaseResponse<LocationDto>.Failure(["Location not found."]);
        }

        var locationDto = _mapper.Map<LocationDto>(location);
        return BaseResponse<LocationDto>.Success(locationDto, "Location retrieved successfully.");
    }
}