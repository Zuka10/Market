using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface ILocationRepository : IGenericRepository<Location>
{
    Task<IEnumerable<Location>> GetActiveLocationsAsync();
    Task<IEnumerable<Location>> GetLocationsByCityAsync(string city);
    Task<Location?> GetLocationWithVendorsAsync(long id);
    Task<IEnumerable<Location>> GetLocationsWithVendorsAsync();
    Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm);
    Task<int> GetVendorCountByLocationAsync(long locationId);
}