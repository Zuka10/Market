using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetAvailableProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId);
    Task<IEnumerable<Product>> GetProductsByLocationAsync(long locationId);
    Task<Product?> GetProductWithDetailsAsync(long id);
    Task<IEnumerable<Product>> GetProductsWithDetailsAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    Task<IEnumerable<Product>> GetOutOfStockProductsAsync();
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(int page, int pageSize, bool? isAvailable = null);
    Task UpdateStockAsync(long productId, int newStock);
}