using Market.Application.Common.Interfaces;
using Market.Application.Common.Models;
using Market.Domain.Abstractions;

namespace Market.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (existingProduct is null)
        {
            return BaseResponse<bool>.Failure(["Product not found."]);
        }

        await _unitOfWork.Products.DeleteAsync(request.ProductId);
        return BaseResponse<bool>.Success(true, "Product deleted successfully.");
    }
}