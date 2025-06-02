using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Vendors.Commands.ActivateVendor;

public class ActivateVendorHandler(IUnitOfWork unitOfWork) : ICommandHandler<ActivateVendorCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(ActivateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId);
        if (vendor is null)
        {
            return BaseResponse<bool>.Failure(["Vendor not found."]);
        }

        if (vendor.IsActive)
        {
            return BaseResponse<bool>.Failure(["Vendor is already active."]);
        }

        vendor.IsActive = true;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Vendors.UpdateAsync(vendor);

        return BaseResponse<bool>.Success(true, "Vendor activated successfully.");
    }
}