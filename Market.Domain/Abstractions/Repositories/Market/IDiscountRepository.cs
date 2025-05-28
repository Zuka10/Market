using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IDiscountRepository : IGenericRepository<Discount>
{
    Task<Discount?> GetByCodeAsync(string code);
    Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
    Task<IEnumerable<Discount>> GetDiscountsByLocationAsync(long locationId);
    Task<IEnumerable<Discount>> GetDiscountsByVendorAsync(long vendorId);
    Task<IEnumerable<Discount>> GetValidDiscountsAsync();
    Task<bool> IsCodeExistsAsync(string code);
}