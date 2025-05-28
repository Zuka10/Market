using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IVendorLocationRepository : IGenericRepository<VendorLocation>
{
    Task<IEnumerable<VendorLocation>> GetActiveVendorLocationsAsync();
    Task<IEnumerable<VendorLocation>> GetByVendorAsync(long vendorId);
    Task<IEnumerable<VendorLocation>> GetByLocationAsync(long locationId);
    Task<VendorLocation?> GetVendorLocationAsync(long vendorId, long locationId);
    Task<IEnumerable<VendorLocation>> GetVendorLocationsWithDetailsAsync();
    Task<bool> IsVendorInLocationAsync(long vendorId, long locationId);
}