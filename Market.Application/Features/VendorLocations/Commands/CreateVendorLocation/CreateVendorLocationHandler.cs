using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.VendorLocations.Commands.CreateVendorLocation;

public class CreateVendorLocationHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateVendorLocationCommand, VendorLocationDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<VendorLocationDto>> Handle(CreateVendorLocationCommand request, CancellationToken cancellationToken)
    {
        var vendorLocation = new VendorLocation
        {
            VendorId = request.VendorId,
            LocationId = request.LocationId,
            StallNumber = request.StallNumber?.Trim(),
            RentAmount = request.RentAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdVendorLocation = await _unitOfWork.VendorLocations.AddAsync(vendorLocation);
        var vendorLocationDto = _mapper.Map<VendorLocationDto>(createdVendorLocation);

        return BaseResponse<VendorLocationDto>.Success(vendorLocationDto, "Vendor-location relationship created successfully.");
    }
}