using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IVendorRepository : IGenericRepository<Vendor>
{
    Task<IEnumerable<Vendor>> GetActiveVendorsAsync();
    Task<Vendor?> GetByEmailAsync(string email);
    Task<Vendor?> GetVendorWithLocationsAsync(long id);
    Task<IEnumerable<Vendor>> GetVendorsWithLocationsAsync();
    Task<IEnumerable<Vendor>> SearchVendorsAsync(string searchTerm);
    Task<IEnumerable<Vendor>> GetVendorsByLocationAsync(long locationId);
    Task<bool> IsEmailExistsAsync(string email);
}