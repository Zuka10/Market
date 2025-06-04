using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Vendors.Commands.DeactivateVendor;

public class DeactivateVendorHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeactivateVendorCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeactivateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.VendorLocations.GetByIdAsync(request.VendorId);
        if (vendor is null)
        {
            return BaseResponse<bool>.Failure(["Vendor not found."]);
        }

        if (!vendor.IsActive)
        {
            return BaseResponse<bool>.Failure(["Vendor is already inactive."]);
        }

        vendor.IsActive = false;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.VendorLocations.UpdateAsync(vendor);

        return BaseResponse<bool>.Success(true, "Vendor deactivated successfully.");
    }
}