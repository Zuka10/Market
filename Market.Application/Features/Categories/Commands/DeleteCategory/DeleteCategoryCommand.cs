using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Categories.Command.DeleteCategory;

public record DeleteCategoryCommand(long CategoryId) : ICommand<bool>;