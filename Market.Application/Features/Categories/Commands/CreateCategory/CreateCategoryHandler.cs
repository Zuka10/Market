using AutoMapper;
using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Domain.Abstractions;
using Market.Domain.Entities.Market;

namespace Market.Application.Features.Categories.Command.CreateCategory;

public class CreateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<BaseResponse<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check if category with same name already exists
        var existingCategory = await _unitOfWork.Categories.GetByNameAsync(request.Name.Trim());
        if (existingCategory is not null)
        {
            return BaseResponse<CategoryDto>.Failure(["Category with this name already exists."]);
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdCategory = await _unitOfWork.Categories.AddAsync(category);
        var categoryDto = _mapper.Map<CategoryDto>(createdCategory);

        return BaseResponse<CategoryDto>.Success(categoryDto, "Category created successfully.");
    }
}