using Market.Domain.Abstractions.Repositories.Auth;
using Market.Domain.Abstractions.Repositories.Market;

namespace Market.Domain.Abstractions;

public interface IUnitOfWork : IDisposable
{
    // Auth Repositories
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }

    // Market Repositories
    IRefreshTokenRepository RefreshTokens { get; }
    ICategoryRepository Categories { get; }
    IDiscountRepository Discounts { get; }
    ILocationRepository Locations { get; }
    IOrderRepository Orders { get; }
    IOrderDetailRepository OrderDetails { get; }
    IPaymentRepository Payments { get; }
    IProcurementRepository Procurements { get; }
    IProcurementDetailRepository ProcurementDetails { get; }
    IProductRepository Products { get; }
    IVendorLocationRepository VendorLocations { get; }
    IVendorRepository Vendors { get; }


    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}