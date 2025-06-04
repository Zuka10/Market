using Market.Domain.Entities.Market;
using Market.Domain.Filters;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface ILocationRepository : IGenericRepository<Location>
{
    Task<IEnumerable<Location>> GetActiveLocationsAsync();
    Task<IEnumerable<Location>> GetLocationsByCityAsync(string city);
    Task<Location?> GetLocationWithVendorsAsync(long id);
    Task<IEnumerable<Location>> GetLocationsWithVendorsAsync();
    Task<int> GetVendorCountByLocationAsync(long locationId);
    Task<PagedResult<Location>> GetLocationsAsync(LocationFilterParameters filterParams);
}