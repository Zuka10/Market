using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Discounts.Commands.DeactivateDiscount;

public class DeactivateDiscountHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeactivateDiscountCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeactivateDiscountCommand request, CancellationToken cancellationToken)
    {
        var existingDiscount = await _unitOfWork.Discounts.GetByIdAsync(request.DiscountId);
        if (existingDiscount is null)
        {
            return BaseResponse<bool>.Failure(["Discount not found."]);
        }

        if (!existingDiscount.IsActive)
        {
            return BaseResponse<bool>.Failure(["Discount is already inactive."]);
        }

        existingDiscount.IsActive = false;
        existingDiscount.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Discounts.UpdateAsync(existingDiscount);
        return BaseResponse<bool>.Success(true, "Discount deactivated successfully.");
    }
}