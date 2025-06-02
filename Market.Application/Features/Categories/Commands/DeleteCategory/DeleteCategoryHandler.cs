using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Categories.Command.DeleteCategory;

public class DeleteCategoryHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteCategoryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategory = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (existingCategory is null)
        {
            return BaseResponse<bool>.Failure(["Category not found."]);
        }

        // Check if category has associated products (business rule check)
        var associatedProducts = await _unitOfWork.Products.GetProductsByCategoryAsync(request.CategoryId);
        if (associatedProducts.Any())
        {
            return BaseResponse<bool>.Failure(["Cannot delete category with associated products. Please remove or reassign products first."]);
        }

        await _unitOfWork.Categories.DeleteAsync(request.CategoryId);
        return BaseResponse<bool>.Success(true, "Category deleted successfully.");
    }
}