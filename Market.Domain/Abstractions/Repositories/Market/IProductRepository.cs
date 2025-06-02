using Market.Domain.Entities.Market;
using Market.Domain.Filters;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetAvailableProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId);
    Task<IEnumerable<Product>> GetProductsByLocationAsync(long locationId);
    Task<Product?> GetProductWithDetailsAsync(long id);
    Task<IEnumerable<Product>> GetProductsWithDetailsAsync();
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    Task<IEnumerable<Product>> GetOutOfStockProductsAsync();
    Task<PagedResult<Product>> GetProductsAsync(ProductFilterParameters filterParams);
    Task UpdateStockAsync(long productId, int newStock);
}