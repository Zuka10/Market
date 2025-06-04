using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Vendors.Commands.UpdateVendor;

public class UpdateVendorHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateVendorCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var existingVendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId);
        if (existingVendor is null)
        {
            return BaseResponse<bool>.Failure(["Vendor not found."]);
        }

        // Check if another vendor with same email exists
        var vendorWithSameEmail = await _unitOfWork.Vendors.GetByEmailAsync(request.Email.Trim());
        if (vendorWithSameEmail is not null && vendorWithSameEmail.Id != request.VendorId)
        {
            return BaseResponse<bool>.Failure(["Another vendor with this email already exists."]);
        }


        // Update vendor properties
        existingVendor.Name = request.Name.Trim();
        existingVendor.Email = request.Email.Trim().ToLowerInvariant();
        existingVendor.PhoneNumber = request.Phone?.Trim()!;
        existingVendor.Address = request.Address?.Trim()!;
        existingVendor.ContactPersonName = request.ContactPerson?.Trim()!;
        existingVendor.IsActive = request.IsActive;
        existingVendor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Vendors.UpdateAsync(existingVendor);
        return BaseResponse<bool>.Success(true, "Vendor updated successfully.");
    }
}