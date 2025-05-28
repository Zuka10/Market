using Market.Domain.Entities.Market;

namespace Market.Domain.Abstractions.Repositories.Market;

public interface IProcurementDetailRepository : IGenericRepository<ProcurementDetail>
{
    Task<IEnumerable<ProcurementDetail>> GetByProcurementAsync(long procurementId);
    Task<IEnumerable<ProcurementDetail>> GetByProductAsync(long productId);
    Task<IEnumerable<ProcurementDetail>> GetProcurementDetailsWithProductsAsync(long procurementId);
}