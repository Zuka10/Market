using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Discounts.Commands.UpdateDiscount;

public class UpdateDiscountHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateDiscountCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
    {
        var existingDiscount = await _unitOfWork.Discounts.GetByIdAsync(request.DiscountId);
        if (existingDiscount is null)
        {
            return BaseResponse<bool>.Failure(["Discount not found."]);
        }

        existingDiscount.DiscountCode = request.DiscountCode.Trim().ToUpperInvariant();
        existingDiscount.Description = request.Description?.Trim();
        existingDiscount.Percentage = request.Percentage;
        existingDiscount.StartDate = request.StartDate;
        existingDiscount.EndDate = request.EndDate;
        existingDiscount.IsActive = request.IsActive;
        existingDiscount.LocationId = request.LocationId;
        existingDiscount.VendorId = request.VendorId;
        existingDiscount.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Discounts.UpdateAsync(existingDiscount);
        return BaseResponse<bool>.Success(true, "Discount updated successfully.");
    }
}