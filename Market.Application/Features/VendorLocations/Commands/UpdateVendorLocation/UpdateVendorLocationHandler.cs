using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Commands.UpdateVendorLocation;

public class UpdateVendorLocationHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateVendorLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateVendorLocationCommand request, CancellationToken cancellationToken)
    {
        var existingVendorLocation = await _unitOfWork.VendorLocations.GetByIdAsync(request.Id);
        if (existingVendorLocation is null)
        {
            return BaseResponse<bool>.Failure(["Vendor-location relationship not found."]);
        }

        existingVendorLocation.VendorId = request.VendorId;
        existingVendorLocation.LocationId = request.LocationId;
        existingVendorLocation.StallNumber = request.StallNumber?.Trim();
        existingVendorLocation.RentAmount = request.RentAmount;
        existingVendorLocation.StartDate = request.StartDate;
        existingVendorLocation.EndDate = request.EndDate;
        existingVendorLocation.IsActive = request.IsActive;
        existingVendorLocation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.VendorLocations.UpdateAsync(existingVendorLocation);
        return BaseResponse<bool>.Success(true, "Vendor-location relationship updated successfully.");
    }
}