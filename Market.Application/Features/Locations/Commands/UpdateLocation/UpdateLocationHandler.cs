using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Locations.Commands.UpdateLocation;

public class UpdateLocationHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var existingLocation = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
        if (existingLocation is null)
        {
            return BaseResponse<bool>.Failure(["Location not found."]);
        }

        // Update location properties
        existingLocation.Name = request.Name.Trim();
        existingLocation.Description = request.Description?.Trim();
        existingLocation.Address = request.Address.Trim();
        existingLocation.City = request.City.Trim();
        existingLocation.Country = request.Country.Trim();
        existingLocation.PostalCode = request.PostalCode?.Trim();
        existingLocation.IsActive = request.IsActive;
        existingLocation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Locations.UpdateAsync(existingLocation);

        return BaseResponse<bool>.Success(true, "Location updated successfully.");
    }
}