using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<Category?> GetCategoryWithProductsAsync(long id);
    Task<IEnumerable<Category>> GetCategoriesWithProductsAsync();
    Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
    Task<int> GetProductCountByCategoryAsync(long categoryId);
    Task<bool> IsNameExistsAsync(string name);
}