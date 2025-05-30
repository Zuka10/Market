using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Locations.Commands.ActivateLocation;

public class ActivateLocationHandler(IUnitOfWork unitOfWork) : ICommandHandler<ActivateLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(ActivateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
        if (location == null)
        {
            return BaseResponse<bool>.Failure(["Location not found."]);
        }

        if (location.IsActive)
        {
            return BaseResponse<bool>.Failure(["Location is already active."]);
        }

        location.IsActive = true;
        location.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Locations.UpdateAsync(location);

        return BaseResponse<bool>.Success(true, "Location activated successfully.");
    }
}
