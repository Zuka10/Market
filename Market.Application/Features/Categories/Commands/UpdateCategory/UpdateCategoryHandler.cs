using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Categories.Command.UpdateCategory;

public class UpdateCategoryHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateCategoryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategory = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (existingCategory is null)
        {
            return BaseResponse<bool>.Failure(["Category not found."]);
        }

        // Check if another category with same name exists
        var categoryWithSameName = await _unitOfWork.Categories.GetByNameAsync(request.Name.Trim());
        if (categoryWithSameName is not null && categoryWithSameName.Id != request.CategoryId)
        {
            return BaseResponse<bool>.Failure(["Another category with this name already exists."]);
        }

        // Update category properties
        existingCategory.Name = request.Name.Trim();
        existingCategory.Description = request.Description?.Trim();
        existingCategory.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(existingCategory);
        return BaseResponse<bool>.Success(true, "Category updated successfully.");
    }
}