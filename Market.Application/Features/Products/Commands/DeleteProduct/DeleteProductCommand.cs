using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(long ProductId) : ICommand<bool>;