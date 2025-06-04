using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Products.Commands.UpdateProductStock;

public record UpdateProductStockCommand(
    long ProductId,
    int NewStock
) : ICommand<bool>;