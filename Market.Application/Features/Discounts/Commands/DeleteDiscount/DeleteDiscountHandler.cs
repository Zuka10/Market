using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Discounts.Commands.DeleteDiscount;

public class DeleteDiscountHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteDiscountCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
    {
        var existingDiscount = await _unitOfWork.Discounts.GetByIdAsync(request.DiscountId);
        if (existingDiscount is null)
        {
            return BaseResponse<bool>.Failure(["Discount not found."]);
        }

        await _unitOfWork.Discounts.DeleteAsync(request.DiscountId);
        return BaseResponse<bool>.Success(true, "Discount deleted successfully.");
    }
}