using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Categories.Command.UpdateCategory;

public record UpdateCategoryCommand(
    long CategoryId,
    string Name,
    string? Description
) : ICommand<bool>;