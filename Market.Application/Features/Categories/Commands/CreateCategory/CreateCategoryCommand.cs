using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Categories.Command.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Description
) : ICommand<CategoryDto>;