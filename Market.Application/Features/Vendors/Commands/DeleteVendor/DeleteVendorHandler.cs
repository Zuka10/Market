using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Vendors.Commands.DeleteVendor;

public class DeleteVendorHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteVendorCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId);
        if (vendor is null)
        {
            return BaseResponse<bool>.Failure(["Vendor not found."]);
        }

        //var hasAssociatedProducts = await _unitOfWork.Products.Get(request.VendorId);
        //if (hasAssociatedProducts)
        //{
        //    return BaseResponse<bool>.Failure(["Cannot delete vendor with associated products. Please remove or reassign products first."]);
        //}

        await _unitOfWork.Vendors.DeleteAsync(vendor.Id);
        return BaseResponse<bool>.Success(true, $"Location '{vendor.Name}' deleted successfully.");
    }
}