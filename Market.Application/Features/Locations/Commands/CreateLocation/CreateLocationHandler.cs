using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.Locations.Commands.CreateLocation;

public class CreateLocationHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateLocationCommand, LocationDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<LocationDto>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        // Create new location
        var location = new Location
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdLocation = await _unitOfWork.Locations.AddAsync(location);
        var locationDto = _mapper.Map<LocationDto>(createdLocation);

        return BaseResponse<LocationDto>.Success(locationDto, "Location created successfully.");
    }
}