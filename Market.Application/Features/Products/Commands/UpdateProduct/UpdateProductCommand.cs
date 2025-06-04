using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    long ProductId,
    string Name,
    string? Description,
    decimal Price,
    int InStock,
    string Unit,
    long LocationId,
    long CategoryId,
    bool IsAvailable
) : ICommand<bool>;