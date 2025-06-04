using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Commands.ExtendContract;

public class ExtendContractHandler(IUnitOfWork unitOfWork) : ICommandHandler<ExtendContractCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(ExtendContractCommand request, CancellationToken cancellationToken)
    {
        var existingVendorLocation = await _unitOfWork.VendorLocations.GetByIdAsync(request.Id);
        if (existingVendorLocation is null)
        {
            return BaseResponse<bool>.Failure(["Vendor-location relationship not found."]);
        }

        if (existingVendorLocation.EndDate.HasValue && request.NewEndDate <= existingVendorLocation.EndDate)
        {
            return BaseResponse<bool>.Failure(["New end date must be later than current end date."]);
        }

        if (!existingVendorLocation.EndDate.HasValue && request.NewEndDate <= existingVendorLocation.StartDate.AddMonths(1))
        {
            return BaseResponse<bool>.Failure(["New end date must be at least 1 month after start date."]);
        }

        existingVendorLocation.EndDate = request.NewEndDate;
        existingVendorLocation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.VendorLocations.UpdateAsync(existingVendorLocation);
        return BaseResponse<bool>.Success(true, "Contract extended successfully.");
    }
}