using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (existingProduct is null)
        {
            return BaseResponse<bool>.Failure(["Product not found."]);
        }

        existingProduct.Name = request.Name.Trim();
        existingProduct.Description = request.Description?.Trim();
        existingProduct.Price = request.Price;
        existingProduct.InStock = request.InStock;
        existingProduct.Unit = request.Unit.Trim().ToLowerInvariant();
        existingProduct.LocationId = request.LocationId;
        existingProduct.CategoryId = request.CategoryId;
        existingProduct.IsAvailable = request.IsAvailable;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(existingProduct);
        return BaseResponse<bool>.Success(true, "Product updated successfully.");
    }
}