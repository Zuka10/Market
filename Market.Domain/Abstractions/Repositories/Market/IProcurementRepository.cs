using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IProcurementRepository : IGenericRepository<Procurement>
{
    Task<Procurement?> GetByReferenceNoAsync(string referenceNo);
    Task<Procurement?> GetProcurementWithDetailsAsync(long id);
    Task<IEnumerable<Procurement>> GetProcurementsWithDetailsAsync();
    Task<IEnumerable<Procurement>> GetProcurementsByVendorAsync(long vendorId);
    Task<IEnumerable<Procurement>> GetProcurementsByLocationAsync(long locationId);
    Task<IEnumerable<Procurement>> GetProcurementsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<(IEnumerable<Procurement> Procurements, int TotalCount)> GetPagedProcurementsAsync(int page, int pageSize);
    Task<decimal> GetTotalProcurementValueAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> IsReferenceNoExistsAsync(string referenceNo);
}