using Market.Application.Common.Interfaces;
using Market.Application.DTOs.Market;

namespace Market.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int InStock,
    string Unit,
    long LocationId,
    long CategoryId,
    bool IsAvailable = true
) : ICommand<ProductDto>;