using Market.Domain.Entities.Market;
using Market.Domain.Filters;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IDiscountRepository : IGenericRepository<Discount>
{
    Task<Discount?> GetByCodeAsync(string code);
    Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
    Task<IEnumerable<Discount>> GetDiscountsByLocationAsync(long locationId);
    Task<IEnumerable<Discount>> GetDiscountsByVendorAsync(long vendorId);
    Task<IEnumerable<Discount>> GetValidDiscountsAsync();
    Task<PagedResult<Discount>> GetDiscountsAsync(DiscountFilterParameters filterParams);
    Task<bool> IsCodeExistsAsync(string code);
}