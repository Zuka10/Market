using Market.Application.Common.Interfaces;

namespace Market.Application.Features.Products.Commands.ActivateProduct;

public record ActivateProductCommand(long ProductId) : ICommand<bool>;