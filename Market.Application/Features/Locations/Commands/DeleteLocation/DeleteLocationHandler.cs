using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Locations.Commands.DeleteLocation;

public class DeleteLocationHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
        if (location == null)
        {
            return BaseResponse<bool>.Failure(["Location not found."]);
        }

        // Check if location has vendors
        var vendorsCount = await _unitOfWork.Locations.GetVendorCountByLocationAsync(request.LocationId);
        if (vendorsCount > 0)
        {
            return BaseResponse<bool>.Failure([$"Cannot delete location '{location.Name}' because it has associated vendors."]);
        }

        await _unitOfWork.Locations.DeleteAsync(request.LocationId);
        return BaseResponse<bool>.Success(true, $"Location '{location.Name}' deleted successfully.");
    }
}