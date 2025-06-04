using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.VendorLocations.Commands.TerminateContract;

public class TerminateContractHandler(IUnitOfWork unitOfWork) : ICommandHandler<TerminateContractCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(TerminateContractCommand request, CancellationToken cancellationToken)
    {
        var existingVendorLocation = await _unitOfWork.VendorLocations.GetByIdAsync(request.Id);
        if (existingVendorLocation is null)
        {
            return BaseResponse<bool>.Failure(["Vendor-location relationship not found."]);
        }

        if (!existingVendorLocation.IsActive)
        {
            return BaseResponse<bool>.Failure(["Contract is already terminated."]);
        }

        var terminationDate = request.TerminationDate ?? DateTime.Today;

        if (terminationDate <= existingVendorLocation.StartDate)
        {
            return BaseResponse<bool>.Failure(["Termination date cannot be before or same as start date."]);
        }

        existingVendorLocation.EndDate = terminationDate;
        existingVendorLocation.IsActive = false;
        existingVendorLocation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.VendorLocations.UpdateAsync(existingVendorLocation);
        return BaseResponse<bool>.Success(true, "Contract terminated successfully.");
    }
}