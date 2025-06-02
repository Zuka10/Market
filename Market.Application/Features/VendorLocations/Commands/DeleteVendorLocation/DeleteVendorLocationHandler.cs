using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Commands.DeleteVendorLocation;

public class DeleteVendorLocationHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteVendorLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteVendorLocationCommand request, CancellationToken cancellationToken)
    {
        var existingVendorLocation = await _unitOfWork.VendorLocations.GetByIdAsync(request.Id);
        if (existingVendorLocation is null)
        {
            return BaseResponse<bool>.Failure(["Vendor-location relationship not found."]);
        }

        await _unitOfWork.VendorLocations.DeleteAsync(request.Id);
        return BaseResponse<bool>.Success(true, "Vendor-location relationship deleted successfully.");
    }
}