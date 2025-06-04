using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Products.Commands.DeactivateProduct;

public class DeactivateProductHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeactivateProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (existingProduct is null)
        {
            return BaseResponse<bool>.Failure(["Product not found."]);
        }

        if (!existingProduct.IsAvailable)
        {
            return BaseResponse<bool>.Failure(["Product is already deactivated."]);
        }

        existingProduct.IsAvailable = false;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(existingProduct);
        return BaseResponse<bool>.Success(true, "Product deactivated successfully.");
    }
}